<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue';
import { api } from '../api';
import type { Pelicula } from '../types';
import { Search, ChevronLeft, ChevronRight } from '@lucide/vue';
import { useRouter } from 'vue-router';

const router = useRouter();
const peliculas = ref<Pelicula[]>([]);
const loading = ref(true);
const searchQuery = ref('');

const currentPage = ref(1);
const itemsPerPage = 6;

const fetchPeliculas = async () => {
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
  let filtered = peliculas.value;
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase();
    filtered = filtered.filter(p => p.nombre.toLowerCase().includes(query));
  }
  return filtered;
});

const totalPages = computed(() => Math.ceil(filteredPeliculas.value.length / itemsPerPage) || 1);

const paginatedPeliculas = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage;
  const end = start + itemsPerPage;
  return filteredPeliculas.value.slice(start, end);
});

watch(searchQuery, () => {
  currentPage.value = 1;
});
</script>

<template>
  <div class="px-6 py-8 max-w-7xl mx-auto space-y-12">
    <!-- Hero / Featured Section -->
    <div class="relative rounded-2xl overflow-hidden bg-gradient-to-r from-[#1a1a1a] to-[#2a2a2a] p-10 md:p-16 border border-[#2a2a2a]">
      <div class="absolute inset-0 bg-cover bg-center opacity-50" style="background-image: url('https://images.unsplash.com/photo-1489599849927-2ee91cede3ba?q=80&w=2070&auto=format&fit=crop');"></div>
      <div class="absolute inset-0 bg-gradient-to-r from-[#121212] to-transparent"></div>
      <div class="relative z-10 max-w-xl">
        <span class="inline-block bg-[#2a2a2a] text-[#d4d444] text-xs font-bold px-3 py-1 rounded-full mb-6 uppercase tracking-wider border border-[#3a3a3a]">
          Featured
        </span>
        <h1 class="text-4xl md:text-5xl font-extrabold text-white mb-6 leading-tight">
          Discover the Art of Cinema
        </h1>
        <p class="text-gray-300 text-lg mb-8 leading-relaxed">
          Read professional reviews, explore the catalog, and join a community of passionate film enthusiasts.
        </p>
        
        <div class="relative w-full max-w-md">
          <input 
            v-model="searchQuery" 
            type="text" 
            placeholder="Search movies..." 
            class="w-full bg-[#121212]/80 backdrop-blur-sm border border-[#3a3a3a] rounded-full pl-6 pr-14 py-4 text-white focus:outline-none focus:border-[#a855f7] focus:ring-1 focus:ring-[#a855f7] transition-all"
          >
          <div class="absolute right-2 top-2 bottom-2 bg-[#a855f7] w-10 h-10 rounded-full flex items-center justify-center cursor-pointer hover:bg-[#9333ea] transition-colors">
            <Search class="w-5 h-5 text-white" />
          </div>
        </div>
      </div>
    </div>

    <!-- Catalog Section -->
    <div>
      <div class="flex items-center justify-between mb-8">
        <h2 class="text-2xl font-bold text-white">Trending This Week</h2>
      </div>

      <div v-if="loading" class="flex justify-center py-20">
        <div class="w-10 h-10 border-4 border-[#a855f7] border-t-transparent rounded-full animate-spin"></div>
      </div>

      <div v-else-if="filteredPeliculas.length === 0" class="text-center py-20 text-gray-400">
        No movies found.
      </div>

      <div v-else class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-6">
        <div 
          v-for="pelicula in paginatedPeliculas" 
          :key="pelicula.id"
          @click="router.push(`/peliculas/${pelicula.id}`)"
          class="group cursor-pointer flex flex-col"
        >
          <div class="relative aspect-[2/3] rounded-xl overflow-hidden mb-3 border border-[#2a2a2a] group-hover:border-[#a855f7] transition-colors">
            <img :src="pelicula.imagen" :alt="pelicula.nombre" class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500" @error="(e) => (e.target as HTMLImageElement).src = 'https://via.placeholder.com/300x450?text=No+Image'">
          </div>
          <h3 class="font-semibold text-white text-sm group-hover:text-[#a855f7] transition-colors line-clamp-1">{{ pelicula.nombre }}</h3>
          <p class="text-gray-400 text-xs mt-1" v-if="pelicula.estreno">{{ pelicula.estreno }}</p>
        </div>
      </div>

      <!-- Pagination -->
      <div v-if="totalPages > 1" class="flex items-center justify-center gap-2 mt-12">
        <button 
          @click="currentPage--" 
          :disabled="currentPage === 1"
          class="p-2 rounded-lg bg-[#2a2a2a] text-white disabled:opacity-50 hover:bg-[#3a3a3a] transition-colors"
        >
          <ChevronLeft class="w-5 h-5" />
        </button>
        
        <button 
          v-for="page in totalPages" 
          :key="page"
          @click="currentPage = page"
          class="w-10 h-10 rounded-lg flex items-center justify-center text-sm font-medium transition-colors"
          :class="currentPage === page ? 'bg-[#a855f7] text-white' : 'bg-[#2a2a2a] text-gray-400 hover:bg-[#3a3a3a] hover:text-white'"
        >
          {{ page }}
        </button>

        <button 
          @click="currentPage++" 
          :disabled="currentPage === totalPages"
          class="p-2 rounded-lg bg-[#2a2a2a] text-white disabled:opacity-50 hover:bg-[#3a3a3a] transition-colors"
        >
          <ChevronRight class="w-5 h-5" />
        </button>
      </div>
    </div>
  </div>
</template>
