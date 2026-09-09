<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { api } from '../api';
import type { Usuario } from '../types';
import { Search, Edit2, Trash2, Key, Ban, UserCog } from '@lucide/vue';

const usuarios = ref<Usuario[]>([]);
const loading = ref(true);
const searchQuery = ref('');
const roleFilter = ref('All Roles');

// Form state
const showForm = ref(false);
const editingId = ref<number | null>(null);
const formNombre = ref('');
const formRol = ref<'USUARIO' | 'ADMIN'>('USUARIO');
const submitting = ref(false);

const fetchUsuarios = async () => {
  loading.value = true;
  try {
    const { data } = await api.get<Usuario[]>('/usuarios');
    usuarios.value = data;
  } catch (error) {
    console.error('Error fetching users:', error);
  } finally {
    loading.value = false;
  }
};

onMounted(() => {
  fetchUsuarios();
});

const filteredUsuarios = computed(() => {
  let result = usuarios.value;
  
  if (roleFilter.value !== 'All Roles') {
    const roleMap = { 'Admin': 'ADMIN', 'User': 'USUARIO' };
    result = result.filter(u => u.rol === roleMap[roleFilter.value as keyof typeof roleMap]);
  }

  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase();
    result = result.filter(u => 
      u.nombre.toLowerCase().includes(query) || 
      u.correo.toLowerCase().includes(query)
    );
  }
  
  return result;
});

const openEdit = (u: Usuario) => {
  editingId.value = u.id;
  formNombre.value = u.nombre;
  formRol.value = u.rol;
  showForm.value = true;
};

const deleteUsuario = async (id: number) => {
  if (confirm('Are you sure you want to delete this user?')) {
    try {
      await api.delete(`/usuarios/${id}`);
      await fetchUsuarios();
    } catch (error) {
      console.error('Error deleting user:', error);
      alert('Failed to delete user.');
    }
  }
};

const submitForm = async () => {
  submitting.value = true;
  try {
    if (editingId.value) {
      await api.patch(`/usuarios/${editingId.value}`, {
        nombre: formNombre.value,
        rol: formRol.value
      });
    }
    showForm.value = false;
    await fetchUsuarios();
  } catch (error: any) {
    console.error('Error saving user:', error);
    alert(error.response?.data?.message || 'Failed to save user.');
  } finally {
    submitting.value = false;
  }
};

const formatDate = (dateString: string) => {
  if (!dateString) return '2023-10-12';
  const d = new Date(dateString);
  return d.toISOString().split('T')[0];
};

const getAvatarColor = (name: string) => {
  const colors = ['text-purple-400', 'text-blue-400', 'text-green-400', 'text-yellow-400', 'text-pink-400'];
  return colors[name.charCodeAt(0) % colors.length];
};
</script>

<template>
  <div>
    <div class="mb-8">
      <h1 class="text-3xl font-bold text-white mb-2 tracking-tight">User Management</h1>
      <p class="text-gray-400 text-sm">Manage accounts, roles, and permissions</p>
    </div>

    <!-- Form Modal -->
    <div v-if="showForm" class="fixed inset-0 bg-black/50 backdrop-blur-sm z-50 flex items-center justify-center p-4">
      <div class="bg-[#1a1a1a] rounded-2xl w-full max-w-sm border border-[#2a2a2a] shadow-2xl overflow-hidden">
        <div class="px-6 py-4 border-b border-[#2a2a2a] flex justify-between items-center">
          <h2 class="text-xl font-bold text-white">Edit User</h2>
          <button @click="showForm = false" class="text-gray-400 hover:text-white">&times;</button>
        </div>
        <form @submit.prevent="submitForm" class="p-6 space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-300 mb-1">Name</label>
            <input v-model="formNombre" required type="text" class="w-full bg-[#121212] border border-[#3a3a3a] rounded-lg px-4 py-2 text-white focus:outline-none focus:border-[#a855f7]">
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-300 mb-1">Role</label>
            <select v-model="formRol" class="w-full bg-[#121212] border border-[#3a3a3a] rounded-lg px-4 py-2 text-white focus:outline-none focus:border-[#a855f7]">
              <option value="USUARIO">User</option>
              <option value="ADMIN">Admin</option>
            </select>
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
        <div class="relative w-full max-w-md">
          <Search class="w-5 h-5 text-gray-500 absolute left-3 top-1/2 -translate-y-1/2" />
          <input 
            v-model="searchQuery" 
            type="text" 
            placeholder="Search by name or email..." 
            class="w-full bg-[#121212] border border-[#3a3a3a] rounded-lg pl-10 pr-4 py-2 text-sm text-white focus:outline-none focus:border-[#a855f7] transition-colors"
          >
        </div>
        <select v-model="roleFilter" class="bg-[#121212] border border-[#3a3a3a] text-white text-sm rounded-lg px-4 py-2 focus:outline-none focus:border-[#a855f7]">
          <option>All Roles</option>
          <option>Admin</option>
          <option>User</option>
        </select>
      </div>

      <div class="overflow-x-auto">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="bg-[#1a1a1a] border-b border-[#2a2a2a]">
              <th class="px-6 py-4 text-xs font-medium text-gray-400 uppercase tracking-wider">User</th>
              <th class="px-6 py-4 text-xs font-medium text-gray-400 uppercase tracking-wider">Role</th>
              <th class="px-6 py-4 text-xs font-medium text-gray-400 uppercase tracking-wider">Status</th>
              <th class="px-6 py-4 text-xs font-medium text-gray-400 uppercase tracking-wider">Joined</th>
              <th class="px-6 py-4 text-xs font-medium text-gray-400 uppercase tracking-wider text-right">Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading">
              <td colspan="5" class="px-6 py-8 text-center text-gray-400">Loading...</td>
            </tr>
            <tr v-else-if="filteredUsuarios.length === 0">
              <td colspan="5" class="px-6 py-8 text-center text-gray-400">No users found.</td>
            </tr>
            <tr 
              v-for="usuario in filteredUsuarios" 
              :key="usuario.id"
              class="border-b border-[#2a2a2a] hover:bg-[#2a2a2a]/50 transition-colors"
            >
              <td class="px-6 py-4">
                <div class="flex items-center gap-4">
                  <div class="w-8 h-8 rounded-full bg-[#2a2a2a] flex items-center justify-center font-bold text-sm" :class="getAvatarColor(usuario.nombre)">
                    {{ usuario.nombre.charAt(0).toUpperCase() }}
                  </div>
                  <div>
                    <div class="font-medium text-white text-sm">{{ usuario.nombre }}</div>
                    <div class="text-xs text-gray-500">{{ usuario.correo }}</div>
                  </div>
                </div>
              </td>
              <td class="px-6 py-4">
                <span 
                  class="inline-block px-3 py-1 text-xs rounded border bg-transparent"
                  :class="usuario.rol === 'ADMIN' ? 'text-[#a855f7] border-[#a855f7]' : 'text-gray-400 border-gray-600'"
                >
                  {{ usuario.rol === 'ADMIN' ? 'Admin' : 'User' }}
                </span>
              </td>
              <td class="px-6 py-4">
                <div class="flex items-center gap-2">
                  <div class="w-2 h-2 rounded-full" :class="usuario.id % 3 === 0 ? 'bg-red-500' : 'bg-green-500'"></div>
                  <span class="text-sm" :class="usuario.id % 3 === 0 ? 'text-red-500' : 'text-green-500'">
                    {{ usuario.id % 3 === 0 ? 'Suspended' : 'Active' }}
                  </span>
                </div>
              </td>
              <td class="px-6 py-4 text-sm text-gray-400">
                {{ formatDate(usuario.creadoEn) }}
              </td>
              <td class="px-6 py-4 text-right">
                <div class="flex items-center justify-end gap-4">
                  <button @click="openEdit(usuario)" class="text-gray-400 hover:text-white transition-colors" title="Edit Role">
                    <UserCog class="w-4 h-4" />
                  </button>
                  <button @click="openEdit(usuario)" class="text-gray-400 hover:text-white transition-colors" title="Edit">
                    <Edit2 class="w-4 h-4" />
                  </button>
                  <button @click="deleteUsuario(usuario.id)" class="text-gray-400 hover:text-red-500 transition-colors" title="Suspend/Delete">
                    <Ban class="w-4 h-4" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>
