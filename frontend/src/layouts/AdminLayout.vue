<script setup lang="ts">
import { useRouter, useRoute } from 'vue-router';
import { Film, Users, LogOut, LayoutGrid } from '@lucide/vue';

const router = useRouter();
const route = useRoute();
</script>

<template>
  <div class="flex min-h-screen bg-[#121212]">
    <!-- Sidebar -->
    <aside class="w-64 bg-[#1a1a1a] border-r border-[#2a2a2a] flex flex-col fixed h-full z-10">
      <div class="p-6 border-b border-[#2a2a2a]">
        <div class="flex items-center gap-2 cursor-pointer" @click="router.push('/peliculas')">
          <Film class="w-6 h-6 text-[#d4d444]" />
          <LayoutGrid class="w-6 h-6 text-[#d4d444]" />
          <span class="text-[#d4d444] font-bold text-xl tracking-tight">Admin Control</span>
        </div>
      </div>
      
      <nav class="flex-grow p-4 space-y-2">
        <router-link 
          to="/admin/peliculas" 
          class="flex items-center gap-3 px-4 py-3 rounded-lg text-sm font-medium transition-colors"
          :class="route.path.includes('/peliculas') ? 'bg-[#a855f7] text-white' : 'text-gray-400 hover:bg-[#2a2a2a] hover:text-white'"
        >
          <Film class="w-5 h-5" />
          Movies
        </router-link>
        
        <router-link 
          to="/admin/usuarios" 
          class="flex items-center gap-3 px-4 py-3 rounded-lg text-sm font-medium transition-colors"
          :class="route.path.includes('/usuarios') ? 'bg-[#a855f7] text-white' : 'text-gray-400 hover:bg-[#2a2a2a] hover:text-white'"
        >
          <Users class="w-5 h-5" />
          Users
        </router-link>
      </nav>

      <div class="p-4 border-t border-[#2a2a2a]">
        <button 
          @click="router.push('/peliculas')" 
          class="w-full flex items-center gap-3 px-4 py-3 rounded-lg text-sm font-medium text-gray-400 hover:bg-[#2a2a2a] hover:text-white transition-colors"
        >
          <LogOut class="w-5 h-5" />
          Exit Admin
        </button>
      </div>
    </aside>

    <!-- Main Content -->
    <main class="flex-grow ml-64 p-8">
      <router-view />
    </main>
  </div>
</template>
