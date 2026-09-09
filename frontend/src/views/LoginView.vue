<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '../stores/auth';
import { api } from '../api';
import { Film, Mail, Lock, ArrowRight } from '@lucide/vue';

const router = useRouter();
const authStore = useAuthStore();

const correo = ref('');
const password = ref('');
const error = ref('');
const loading = ref(false);

const handleLogin = async () => {
  error.value = '';
  loading.value = true;
  try {
    const { data } = await api.post('/auth/login', {
      correo: correo.value,
      password: password.value
    });
    authStore.setAuth(data.access_token, data.usuario);
    router.push('/peliculas');
  } catch (err: any) {
    error.value = err.response?.data?.message || 'Error al iniciar sesión';
  } finally {
    loading.value = false;
  }
};
</script>

<template>
  <div class="min-h-screen flex items-center justify-center bg-[#121212] px-4 relative overflow-hidden">
    <!-- Background glows -->
    <div class="absolute top-0 left-0 w-[500px] h-[500px] bg-[#a855f7]/20 rounded-full blur-[120px] -translate-x-1/2 -translate-y-1/2"></div>
    <div class="absolute bottom-0 right-0 w-[500px] h-[500px] bg-[#d4d444]/10 rounded-full blur-[120px] translate-x-1/3 translate-y-1/3"></div>

    <div class="w-full max-w-[400px] bg-[#1a1a1a] rounded-2xl p-8 border border-[#2a2a2a] shadow-2xl relative z-10">
      <div class="flex flex-col items-center mb-8">
        <div class="w-12 h-12 rounded-xl bg-[#2a2a2a] flex items-center justify-center mb-4 border border-[#3a3a3a]">
          <Film class="w-6 h-6 text-[#a855f7]" />
        </div>
        <h1 class="text-2xl font-bold text-white tracking-tight">Welcome back</h1>
        <p class="text-gray-400 mt-2 text-sm text-center">Enter your details to access your account.</p>
      </div>

      <form @submit.prevent="handleLogin" class="space-y-5">
        <div v-if="error" class="bg-red-500/10 border border-red-500/50 text-red-500 p-3 rounded-lg text-sm text-center">
          {{ error }}
        </div>

        <div>
          <label class="block text-sm font-medium text-gray-300 mb-1.5">Email</label>
          <div class="relative">
            <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <Mail class="w-5 h-5 text-gray-500" />
            </div>
            <input 
              v-model="correo" 
              type="email" 
              required 
              class="w-full bg-[#121212] border border-[#3a3a3a] rounded-lg pl-10 pr-4 py-2.5 text-white focus:outline-none focus:border-[#a855f7] transition-colors text-sm"
              placeholder="name@example.com"
            >
          </div>
        </div>

        <div>
          <div class="flex items-center justify-between mb-1.5">
            <label class="block text-sm font-medium text-gray-300">Password</label>
            <a href="#" class="text-sm text-[#a855f7] hover:text-[#9333ea] transition-colors">Forgot password?</a>
          </div>
          <div class="relative">
            <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <Lock class="w-5 h-5 text-gray-500" />
            </div>
            <input 
              v-model="password" 
              type="password" 
              required 
              class="w-full bg-[#121212] border border-[#3a3a3a] rounded-lg pl-10 pr-4 py-2.5 text-white focus:outline-none focus:border-[#a855f7] transition-colors text-sm"
              placeholder="••••••••"
            >
          </div>
        </div>

        <button 
          type="submit" 
          :disabled="loading"
          class="w-full bg-[#a855f7] hover:bg-[#9333ea] text-white font-medium py-2.5 rounded-lg transition-colors disabled:opacity-50 mt-2 flex items-center justify-center gap-2"
        >
          {{ loading ? 'Signing in...' : 'Sign In' }}
          <ArrowRight v-if="!loading" class="w-4 h-4" />
        </button>
      </form>

      <p class="text-center text-gray-400 text-sm mt-8">
        Don't have an account? 
        <router-link to="/registro" class="text-[#d4d444] hover:underline font-medium">Sign up</router-link>
      </p>
    </div>
  </div>
</template>
