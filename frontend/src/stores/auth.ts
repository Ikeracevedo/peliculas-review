import { defineStore } from 'pinia';
import { ref } from 'vue';

export interface User {
  id: number;
  correo: string;
  nombre: string;
  rol: 'USUARIO' | 'ADMIN';
}

export const useAuthStore = defineStore('auth', () => {
  const token = ref(localStorage.getItem('token') || null);
  const user = ref<User | null>(JSON.parse(localStorage.getItem('user') || 'null'));

  const setAuth = (newToken: string, newUser: User) => {
    token.value = newToken;
    user.value = newUser;
    localStorage.setItem('token', newToken);
    localStorage.setItem('user', JSON.stringify(newUser));
  };

  const logout = () => {
    token.value = null;
    user.value = null;
    localStorage.removeItem('token');
    localStorage.removeItem('user');
  };

  return { token, user, setAuth, logout };
});
