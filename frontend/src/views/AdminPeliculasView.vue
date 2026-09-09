<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { api } from '../api';
import type { Pelicula } from '../types';
import { Plus, Search, Edit2, Trash2, MoreHorizontal, ChevronLeft, ChevronRight } from '@lucide/vue';

const peliculas = ref<Pelicula[]>([]);
const loading = ref(true);
const searchQuery = ref('');

// Form state
const showForm = ref(false);
const editingId = ref<number | null>(null);
const formNombre = ref('');
const formImagen = ref('');
const formEstreno = ref<number | null>(null);
const formDescripcion = ref('');
const submitting = ref(false);

// Pagination
const currentPage = ref(1);
const itemsPerPage = 5;

const fetchPeliculas = async () => {
  loading.value = true;
  try {
    const { data } = await api.get<Pelicula[]>('/peliculas');
    peliculas.value = data;
  } catch (error) {
    console.error('Error fetching peliculas:', error);
  } finally {
    loading.value = false;
  }
};

onMounted(() => {
  fetchPeliculas();
});

const filteredPeliculas = computed(() => {
  if (!searchQuery.value) return peliculas.value;
  return peliculas.value.filter(p => p.nombre.toLowerCase().includes(searchQuery.value.toLowerCase()));
});

const totalPages = computed(() => Math.ceil(filteredPeliculas.value.length / itemsPerPage) || 1);

const paginatedPeliculas = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage;
  return filteredPeliculas.value.slice(start, start + itemsPerPage);
});

const openCreate = () => {
  editingId.value = null;
  formNombre.value = '';
  formImagen.value = '';
  formEstreno.value = new Date().getFullYear();
  formDescripcion.value = '';
  showForm.value = true;
};

const openEdit = (p: Pelicula) => {
  editingId.value = p.id;
  formNombre.value = p.nombre;
  formImagen.value = p.imagen;
  formEstreno.value = p.estreno;
  formDescripcion.value = p.descripcion || '';
  showForm.value = true;
};

const deletePelicula = async (id: number) => {
  if (confirm('Are you sure you want to delete this movie?')) {
    try {
      await api.delete(`/peliculas/${id}`);
      await fetchPeliculas();
    } catch (error) {
      console.error('Error deleting movie:', error);
      alert('Failed to delete movie.');
    }
  }
};

const submitForm = async () => {
  submitting.value = true;
  try {
    const payload = {
      nombre: formNombre.value,
      imagen: formImagen.value,
      estreno: formEstreno.value || undefined,
      descripcion: formDescripcion.value || undefined,
    };
    if (editingId.value) {
      await api.patch(`/peliculas/${editingId.value}`, payload);
    } else {
      await api.post('/peliculas', payload);
    }
    showForm.value = false;
    await fetchPeliculas();
  } catch (error: any) {
    console.error('Error saving movie:', error);
    alert(error.response?.data?.message || 'Failed to save movie.');
  } finally {
    submitting.value = false;
  }
};
</script>

<template>
  <div>
    <div class="flex items-center justify-between mb-8">
      <div>
        <h1 class="text-3xl font-bold text-white mb-2">Movies Management</h1>
        <p class="text-gray-400">Manage the platform's movie catalog</p>
      </div>
      <button @click="openCreate" class="bg-[#a855f7] hover:bg-[#9333ea] text-white px-4 py-2 rounded-lg font-medium flex items-center gap-2 transition-colors">
        <Plus class="w-5 h-5" />
        Add Movie
      </button>
    </div>

    <!-- Form Modal -->
    <div v-if="showForm" class="fixed inset-0 bg-black/50 backdrop-blur-sm z-50 flex items-center justify-center p-4">
      <div class="bg-[#1a1a1a] rounded-2xl w-full max-w-lg border border-[#2a2a2a] shadow-2xl overflow-hidden">
        <div class="px-6 py-4 border-b border-[#2a2a2a] flex justify-between items-center">
          <h2 class="text-xl font-bold text-white">{{ editingId ? 'Edit Movie' : 'Add Movie' }}</h2>
          <button @click="showForm = false" class="text-gray-400 hover:text-white">&times;</button>
        </div>
        <form @submit.prevent="submitForm" class="p-6 space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-300 mb-1">Title</label>
            <input v-model="formNombre" required type="text" class="w-full bg-[#2a2a2a] border border-[#3a3a3a] rounded-lg px-4 py-2 text-white focus:outline-none focus:border-[#a855f7]">
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-300 mb-1">Image URL</label>
            <input v-model="formImagen" required type="url" class="w-full bg-[#2a2a2a] border border-[#3a3a3a] rounded-lg px-4 py-2 text-white focus:outline-none focus:border-[#a855f7]">
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-300 mb-1">Year</label>
            <input v-model.number="formEstreno" type="number" class="w-full bg-[#2a2a2a] border border-[#3a3a3a] rounded-lg px-4 py-2 text-white focus:outline-none focus:border-[#a855f7]">
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-300 mb-1">Synopsis</label>
            <textarea v-model="formDescripcion" rows="3" class="w-full bg-[#2a2a2a] border border-[#3a3a3a] rounded-lg px-4 py-2 text-white focus:outline-none focus:border-[#a855f7]"></textarea>
          </div>
          <div class="flex justify-end gap-3 pt-4">
            <button type="button" @click="showForm = false" class="px-4 py-2 text-gray-400 hover:text-white">Cancel</button>
            <button type="submit" :disabled="submitting" class="bg-[#a855f7] hover:bg-[#9333ea] text-white px-6 py-2 rounded-lg font-medium transition-colors disabled:opacity-50">
              {{ submitting ? 'Saving...' : 'Save' }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Table Section -->
    <div class="bg-[#1a1a1a] rounded-xl border border-[#2a2a2a] overflow-hidden">
      <div class="p-4 border-b border-[#2a2a2a] flex items-center justify-between gap-4">
        <div class="relative w-full max-w-sm">
          <Search class="w-5 h-5 text-gray-400 absolute left-3 top-1/2 -translate-y-1/2" />
          <input 
            v-model="searchQuery" 
            type="text" 
            placeholder="Search movies..." 
            class="w-full bg-[#121212] border border-[#3a3a3a] rounded-lg pl-10 pr-4 py-2 text-sm text-white focus:outline-none focus:border-[#a855f7] transition-colors"
          >
        </div>
      </div>

      <div class="overflow-x-auto">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="bg-[#121212] border-b border-[#2a2a2a]">
              <th class="px-6 py-4 text-sm font-medium text-gray-400">Title</th>
              <th class="px-6 py-4 text-sm font-medium text-gray-400">Year</th>
              <th class="px-6 py-4 text-sm font-medium text-gray-400 text-right">Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading">
              <td colspan="3" class="px-6 py-8 text-center text-gray-400">Loading...</td>
            </tr>
            <tr v-else-if="paginatedPeliculas.length === 0">
              <td colspan="3" class="px-6 py-8 text-center text-gray-400">No movies found.</td>
            </tr>
            <tr 
              v-for="pelicula in paginatedPeliculas" 
              :key="pelicula.id"
              class="border-b border-[#2a2a2a] hover:bg-[#2a2a2a]/50 transition-colors"
            >
              <td class="px-6 py-4">
                <div class="font-medium text-white">{{ pelicula.nombre }}</div>
              </td>
              <td class="px-6 py-4 text-gray-400">{{ pelicula.estreno || '-' }}</td>
              <td class="px-6 py-4 text-right">
                <div class="flex items-center justify-end gap-2">
                  <button @click="openEdit(pelicula)" class="p-2 text-gray-400 hover:text-white transition-colors" title="Edit">
                    <Edit2 class="w-4 h-4" />
                  </button>
                  <button @click="deletePelicula(pelicula.id)" class="p-2 text-gray-400 hover:text-red-500 transition-colors" title="Delete">
                    <Trash2 class="w-4 h-4" />
                  </button>
                  <button class="p-2 text-gray-400 hover:text-white transition-colors" title="More">
                    <MoreHorizontal class="w-4 h-4" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination Footer -->
      <div class="px-6 py-4 border-t border-[#2a2a2a] flex items-center justify-between">
        <span class="text-sm text-gray-400">
          Showing {{ ((currentPage - 1) * itemsPerPage) + 1 }} to {{ Math.min(currentPage * itemsPerPage, filteredPeliculas.length) }} of {{ filteredPeliculas.length }} entries
        </span>
        
        <div v-if="totalPages > 1" class="flex items-center gap-1">
          <button @click="currentPage--" :disabled="currentPage === 1" class="px-3 py-1 text-sm text-gray-400 hover:text-white disabled:opacity-50 transition-colors">Prev</button>
          
          <button 
            v-for="page in totalPages" 
            :key="page"
            @click="currentPage = page"
            class="w-8 h-8 rounded text-sm font-medium transition-colors flex items-center justify-center"
            :class="currentPage === page ? 'bg-[#a855f7] text-white' : 'text-gray-400 hover:bg-[#2a2a2a] hover:text-white'"
          >
            {{ page }}
          </button>
          
          <button @click="currentPage++" :disabled="currentPage === totalPages" class="px-3 py-1 text-sm text-gray-400 hover:text-white disabled:opacity-50 transition-colors">Next</button>
        </div>
      </div>
    </div>
  </div>
</template>
