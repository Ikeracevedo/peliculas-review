import { createRouter, createWebHistory } from 'vue-router';
import { useAuthStore } from '../stores/auth';
import MainLayout from '../layouts/MainLayout.vue';

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/login',
      name: 'Login',
      component: () => import('../views/LoginView.vue'),
    },
    {
      path: '/registro',
      name: 'Registro',
      component: () => import('../views/RegisterView.vue'),
    },
    {
      path: '/',
      component: MainLayout,
      children: [
        {
          path: '',
          redirect: '/peliculas'
        },
        {
          path: 'peliculas',
          name: 'Peliculas',
          component: () => import('../views/PeliculasView.vue'),
          meta: { requiresAuth: true }
        },
        {
          path: 'peliculas/:id',
          name: 'PeliculaDetail',
          component: () => import('../views/PeliculaDetailView.vue'),
          meta: { requiresAuth: true }
        }
      ]
    },
    {
      path: '/admin',
      component: () => import('../layouts/AdminLayout.vue'),
      children: [
        {
          path: 'peliculas',
          name: 'AdminPeliculas',
          component: () => import('../views/AdminPeliculasView.vue'),
          meta: { requiresAuth: true, requiresAdmin: true }
        },
        {
          path: 'usuarios',
          name: 'AdminUsuarios',
          component: () => import('../views/AdminUsuariosView.vue'),
          meta: { requiresAuth: true, requiresAdmin: true }
        }
      ]
    }
  ],
});

router.beforeEach((to, from, next) => {
  const authStore = useAuthStore();
  
  if (to.meta.requiresAuth && !authStore.token) {
    return next('/login');
  }
  
  if (to.meta.requiresAdmin && authStore.user?.rol !== 'ADMIN') {
    return next('/');
  }
  
  if ((to.name === 'Login' || to.name === 'Registro') && authStore.token) {
    return next('/');
  }
  
  next();
});

export default router;
