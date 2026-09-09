<script setup lang="ts">
import { computed } from 'vue';
import { useWatchlistStore } from '../stores/watchlist';
import { useRouter } from 'vue-router';
import { Trash2, BookmarkX } from '@lucide/vue';

const watchlistStore = useWatchlistStore();
const router = useRouter();

const peliculas = computed(() => watchlistStore.watchlist);
</script>

<template>
  <div class="px-6 py-8 max-w-7xl mx-auto">
    <div class="mb-8">
      <h1 class="text-3xl font-bold text-white mb-2">Mi Watchlist</h1>
      <p class="text-gray-400">Peliculas que quieres ver</p>
    </div>

    <div v-if="peliculas.length === 0" class="flex flex-col items-center justify-center py-24 text-center">
      <BookmarkX class="w-16 h-16 text-gray-600 mb-4" />
      <h2 class="text-xl font-semibold text-gray-400 mb-2">Tu watchlist esta vacia</h2>
      <p class="text-gray-500 mb-6">Añade peliculas desde el catalogo para guardarlas aqui.</p>
      <button @click="router.push('/peliculas')" class="bg-[#a855f7] hover:bg-[#9333ea] text-white px-6 py-3 rounded-lg font-medium transition-colors">
        Explorar catalogo
      </button>
    </div>

    <div v-else class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-6">
      <div
        v-for="pelicula in peliculas"
        :key="pelicula.id"
        class="group relative flex flex-col cursor-pointer"
      >
        <div
          class="relative aspect-[2/3] rounded-xl overflow-hidden mb-3 border border-[#2a2a2a] group-hover:border-[#a855f7] transition-colors"
          @click="router.push(/peliculas/)"
        >
          <img
            :src="pelicula.imagen"
            :alt="pelicula.nombre"
            class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
            @error="(e) => (e.target as HTMLImageElement).src = 'https://via.placeholder.com/300x450?text=No+Image'"
          >
          <button
            @click.stop="watchlistStore.remove(pelicula.id)"
            class="absolute top-2 right-2 bg-black/60 hover:bg-red-600 text-white p-2 rounded-full opacity-0 group-hover:opacity-100 transition-all"
            title="Quitar de watchlist"
          >
            <Trash2 class="w-4 h-4" />
          </button>
        </div>
        <h3 class="font-semibold text-white text-sm line-clamp-1">{{ pelicula.nombre }}</h3>
        <p class="text-gray-400 text-xs mt-1" v-if="pelicula.estreno">{{ pelicula.estreno }}</p>
        <p v-if="pelicula.sinopsis" class="text-gray-500 text-xs mt-1 line-clamp-2">{{ pelicula.sinopsis }}</p>
      </div>
    </div>
  </div>
</template>
