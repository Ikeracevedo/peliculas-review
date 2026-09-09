<script setup lang="ts">
import { useAuthStore } from '../stores/auth';
import { useRouter } from 'vue-router';
import { Film, LogOut, User as UserIcon } from '@lucide/vue';

const authStore = useAuthStore();
const router = useRouter();

const handleLogout = () => {
  authStore.logout();
  router.push('/login');
};
</script>

<template>
  <div class="min-h-screen flex flex-col">
    <header class="bg-[#121212] border-b border-[#2a2a2a] px-6 py-4 flex items-center justify-between sticky top-0 z-50">
      <div class="flex items-center gap-2 cursor-pointer" @click="router.push('/peliculas')">
        <Film class="w-6 h-6 text-[#a855f7]" />
        <span class="text-[#a855f7] font-bold text-xl tracking-tight">CineVault</span>
      </div>
      
      <nav class="flex items-center gap-6 text-sm font-medium">
        <router-link to="/peliculas" class="text-gray-300 hover:text-white transition-colors" active-class="!text-white font-semibold">Catalog</router-link>
        
        <router-link 
          v-if="authStore.user?.rol === 'ADMIN'" 
          to="/admin/peliculas" 
          class="text-gray-300 hover:text-white transition-colors flex items-center gap-1"
          active-class="!text-white font-semibold"
        >
          <span>Admin</span>
        </router-link>

        <div v-if="authStore.user" class="flex items-center gap-4 ml-2 border-l border-[#2a2a2a] pl-6">
          <div class="flex items-center gap-2 text-gray-300">
            <div class="w-8 h-8 rounded-full bg-[#2a2a2a] flex items-center justify-center">
              <span class="text-xs">{{ authStore.user.nombre.charAt(0).toUpperCase() }}</span>
            </div>
          </div>
          <button @click="handleLogout" class="flex items-center gap-2 bg-[#2a2a2a] hover:bg-[#3a3a3a] px-4 py-2 rounded-full transition-colors text-white">
            <span class="text-xs">Logout</span>
          </button>
        </div>
        <div v-else class="ml-2 border-l border-[#2a2a2a] pl-6">
          <router-link to="/login" class="flex items-center gap-2 bg-[#2a2a2a] hover:bg-[#3a3a3a] px-4 py-2 rounded-full transition-colors text-white">
            <UserIcon class="w-4 h-4 text-[#a855f7]" />
            <span>Sign In</span>
          </router-link>
        </div>
      </nav>
    </header>

    <main class="flex-grow">
      <router-view />
    </main>
  </div>
</template>
