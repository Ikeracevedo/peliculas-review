<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRoute } from 'vue-router';
import { api } from '../api';
import type { Pelicula, Review } from '../types';
import { useAuthStore } from '../stores/auth';
import { useWatchlistStore } from '../stores/watchlist';
import { Play, Plus, Share2, Star, Trash2, Edit2, Check } from '@lucide/vue';

const route = useRoute();
const authStore = useAuthStore();
const watchlistStore = useWatchlistStore();
const peliculaId = Number(route.params.id);

const pelicula = ref<Pelicula | null>(null);
const reviews = ref<Review[]>([]);
const loading = ref(true);

const showForm = ref(false);
const formTitle = ref('');
const formContent = ref('');
const formRating = ref(5);
const editingReviewId = ref<number | null>(null);
const submitting = ref(false);

const fetchData = async () => {
  try {
    const [peliRes, reviewsRes] = await Promise.all([
      api.get<Pelicula>(`/peliculas/${peliculaId}`),
      api.get<Review[]>('/reviews')
    ]);
    pelicula.value = peliRes.data;
    reviews.value = reviewsRes.data.filter(r => r.peliculaId === peliculaId);
  } catch (error) {
    console.error('Error fetching data:', error);
  } finally {
    loading.value = false;
  }
};

onMounted(() => { fetchData(); });

const openCreateForm = () => {
  showForm.value = true;
  editingReviewId.value = null;
  formTitle.value = '';
  formContent.value = '';
  formRating.value = 5;
};

const openEditForm = (review: Review) => {
  showForm.value = true;
  editingReviewId.value = review.id;
  formTitle.value = review.titulo;
  formContent.value = review.contenido;
  formRating.value = review.calificacion;
};

const submitReview = async () => {
  submitting.value = true;
  try {
    if (editingReviewId.value) {
      await api.patch(`/reviews/${editingReviewId.value}`, {
        titulo: formTitle.value, contenido: formContent.value, calificacion: formRating.value,
      });
    } else {
      await api.post('/reviews', {
        titulo: formTitle.value, contenido: formContent.value, calificacion: formRating.value, peliculaId,
      });
    }
    showForm.value = false;
    await fetchData();
  } catch (error) {
    console.error(error);
    alert('Error al guardar la resena.');
  } finally {
    submitting.value = false;
  }
};

const deleteReview = async (id: number) => {
  if (confirm('Confirmar eliminacion de resena')) {
    try {
      await api.delete(`/reviews/${id}`);
      await fetchData();
    } catch (error) {
      alert('Error al eliminar.');
    }
  }
};

const formatDate = (dateString: string) => new Date(dateString).toLocaleDateString('es-ES');

const canEdit = (review: Review) =>
  authStore.user?.rol === 'ADMIN' || authStore.user?.id === review.autorId;

const handleWatchTrailer = () => {
  if (pelicula.value) {
    const q = encodeURIComponent(`${pelicula.value.nombre} official trailer`);
    window.open(`https://www.youtube.com/results?search_query=${q}`, '_blank');
  }
};

const handleAddToList = () => {
  if (!pelicula.value) return;
  if (watchlistStore.isInWatchlist(pelicula.value.id)) {
    watchlistStore.remove(pelicula.value.id);
  } else {
    watchlistStore.add(pelicula.value);
  }
};
</script>

<template>
  <div v-if="loading" class="flex justify-center items-center min-h-[60vh]">
    <div class="w-10 h-10 border-4 border-[#a855f7] border-t-transparent rounded-full animate-spin"></div>
  </div>

  <div v-else-if="pelicula">
    <div class="relative bg-[#1a1a1a] border-b border-[#2a2a2a]">
      <div class="absolute inset-0 bg-gradient-to-r from-[#121212] via-[#121212]/80 to-transparent z-10"></div>
      <img :src="pelicula.imagen" class="absolute inset-0 w-full h-full object-cover opacity-20" alt="Banner"
        @error="(e) => (e.target as HTMLImageElement).src = 'https://via.placeholder.com/1200x400?text=No+Image'">

      <div class="relative z-20 max-w-7xl mx-auto px-6 py-12 md:py-20 flex flex-col md:flex-row gap-8 md:gap-12 items-start">
        <img :src="pelicula.imagen" :alt="pelicula.nombre"
          class="w-48 md:w-64 rounded-xl shadow-2xl border border-[#3a3a3a] shrink-0"
          @error="(e) => (e.target as HTMLImageElement).src = 'https://via.placeholder.com/300x450?text=No+Image'">

        <div class="flex-grow pt-4">
          <h1 class="text-4xl md:text-5xl font-bold text-white mb-4">{{ pelicula.nombre }}</h1>

          <div class="flex items-center gap-4 text-sm text-gray-300 mb-8">
            <div class="flex items-center gap-1 text-[#d4d444] font-medium">
              <Star class="w-4 h-4 fill-current" />
              <span>{{ reviews.length > 0 ? (reviews.reduce((a, r) => a + r.calificacion, 0) / reviews.length).toFixed(1) : 'N/A' }}</span>
            </div>
            <span v-if="pelicula.estreno">{{ pelicula.estreno }}</span>
          </div>

          <div class="flex flex-wrap items-center gap-4 mb-8">
            <button @click="handleWatchTrailer"
              class="bg-[#a855f7] hover:bg-[#9333ea] text-white px-6 py-3 rounded-lg font-medium flex items-center gap-2 transition-colors">
              <Play class="w-5 h-5 fill-current" /> Ver Trailer
            </button>
            <button @click="handleAddToList"
              class="px-6 py-3 rounded-lg font-medium flex items-center gap-2 transition-colors border"
              :class="watchlistStore.isInWatchlist(pelicula.id)
                ? 'bg-[#a855f7]/20 border-[#a855f7] text-[#a855f7]'
                : 'bg-[#2a2a2a] hover:bg-[#3a3a3a] text-white border-[#3a3a3a]'">
              <Check v-if="watchlistStore.isInWatchlist(pelicula.id)" class="w-5 h-5" />
              <Plus v-else class="w-5 h-5" />
              {{ watchlistStore.isInWatchlist(pelicula.id) ? 'En Watchlist' : 'Anadir a la lista' }}
            </button>
          </div>

          <div v-if="pelicula.sinopsis">
            <h3 class="text-white font-semibold mb-2">Sinopsis</h3>
            <p class="text-gray-300 leading-relaxed max-w-3xl">{{ pelicula.sinopsis }}</p>
          </div>
        </div>
      </div>
    </div>

    <div class="max-w-7xl mx-auto px-6 py-16">
      <div class="flex items-center justify-between mb-8">
        <h2 class="text-2xl font-bold text-white">Resenas de usuarios</h2>
        <button @click="openCreateForm" class="text-[#a855f7] hover:text-[#9333ea] font-medium transition-colors">
          Escribir resena
        </button>
      </div>

      <div v-if="showForm" class="bg-[#1a1a1a] border border-[#2a2a2a] rounded-xl p-6 mb-8">
        <h3 class="text-lg font-semibold text-white mb-4">{{ editingReviewId ? 'Editar resena' : 'Nueva resena' }}</h3>
        <form @submit.prevent="submitReview" class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-300 mb-1">Titulo</label>
            <input v-model="formTitle" required type="text" class="w-full bg-[#2a2a2a] border border-[#3a3a3a] rounded-lg px-4 py-2 text-white focus:outline-none focus:border-[#a855f7]">
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-300 mb-1">Calificacion (1-5)</label>
            <input v-model.number="formRating" required type="number" min="1" max="5" class="w-full bg-[#2a2a2a] border border-[#3a3a3a] rounded-lg px-4 py-2 text-white focus:outline-none focus:border-[#a855f7]">
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-300 mb-1">Contenido</label>
            <textarea v-model="formContent" required rows="4" class="w-full bg-[#2a2a2a] border border-[#3a3a3a] rounded-lg px-4 py-2 text-white focus:outline-none focus:border-[#a855f7]"></textarea>
          </div>
          <div class="flex gap-4 justify-end">
            <button type="button" @click="showForm = false" class="px-4 py-2 text-gray-400 hover:text-white transition-colors">Cancelar</button>
            <button type="submit" :disabled="submitting" class="bg-[#a855f7] hover:bg-[#9333ea] text-white px-6 py-2 rounded-lg font-medium transition-colors disabled:opacity-50">
              {{ submitting ? 'Guardando...' : 'Guardar resena' }}
            </button>
          </div>
        </form>
      </div>

      <div v-if="reviews.length === 0 && !showForm" class="text-gray-400 py-8">
        Sin resenas todavia.
      </div>

      <div class="grid md:grid-cols-2 gap-6">
        <div v-for="review in reviews" :key="review.id" class="bg-[#1a1a1a] border border-[#2a2a2a] rounded-xl p-6">
          <div class="flex justify-between items-start mb-4">
            <div class="flex items-center gap-3">
              <div class="w-10 h-10 rounded-full bg-[#2a2a2a] flex items-center justify-center text-white font-medium">
                {{ review.autor?.nombre?.charAt(0).toUpperCase() || 'U' }}
              </div>
              <div>
                <h4 class="text-white font-medium text-sm">{{ review.autor?.nombre || 'Usuario' }}</h4>
                <p class="text-gray-500 text-xs">{{ formatDate(review.creadoEn) }}</p>
              </div>
            </div>
            <div class="flex items-center gap-1 text-[#d4d444]">
              <Star v-for="i in 5" :key="i" class="w-4 h-4" :class="i <= review.calificacion ? 'fill-current' : 'text-gray-600'" />
            </div>
          </div>
          <h5 class="text-white font-medium mb-2">{{ review.titulo }}</h5>
          <p class="text-gray-400 text-sm leading-relaxed mb-4">{{ review.contenido }}</p>

          <div v-if="canEdit(review)" class="flex gap-3 justify-end pt-4 border-t border-[#2a2a2a]">
            <button @click="openEditForm(review)" class="text-gray-400 hover:text-white transition-colors flex items-center gap-1 text-xs">
              <Edit2 class="w-4 h-4" /> Editar
            </button>
            <button @click="deleteReview(review.id)" class="text-gray-400 hover:text-red-500 transition-colors flex items-center gap-1 text-xs">
              <Trash2 class="w-4 h-4" /> Eliminar
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>

  <div v-else class="text-center py-20 text-gray-400">Pelicula no encontrada.</div>
</template>