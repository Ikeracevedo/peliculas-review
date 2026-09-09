import { defineStore } from 'pinia';
import { ref } from 'vue';
import type { Pelicula } from '../types';

export const useWatchlistStore = defineStore('watchlist', () => {
  const watchlist = ref<Pelicula[]>([]);

  const load = () => {
    try {
      const stored = localStorage.getItem('watchlist');
      if (stored) watchlist.value = JSON.parse(stored);
    } catch {
      watchlist.value = [];
    }
  };

  const save = () => {
    localStorage.setItem('watchlist', JSON.stringify(watchlist.value));
  };

  const add = (pelicula: Pelicula) => {
    if (!watchlist.value.find(p => p.id === pelicula.id)) {
      watchlist.value.push(pelicula);
      save();
    }
  };

  const remove = (peliculaId: number) => {
    watchlist.value = watchlist.value.filter(p => p.id !== peliculaId);
    save();
  };

  const isInWatchlist = (peliculaId: number) => {
    return watchlist.value.some(p => p.id === peliculaId);
  };

  load();

  return { watchlist, add, remove, isInWatchlist };
});
