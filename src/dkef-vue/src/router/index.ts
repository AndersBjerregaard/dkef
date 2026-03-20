import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '@/views/HomeView.vue'
import { useAuthStore } from '@/stores/authStore'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomeView,
    },
    {
      path: '/about',
      name: 'about',
      // route level code-splitting
      // this generates a separate chunk (About.[hash].js) for this route
      // which is lazy-loaded when the route is visited.
      component: () => import('@/views/AboutView.vue'),
    },
    {
      path: '/advantages',
      name: 'advantages',
      component: () => import('@/views/MemberAdvantagesView.vue'),
    },
    {
      path: '/events-and-news',
      name: 'events-and-news',
      component: () => import('@/views/EventsAndNewsView.vue'),
    },
    {
      path: '/events-and-news/:id',
      name: 'SpecificEvent',
      component: () => import('@/views/SpecificEventView.vue'),
      props: true,
    },
    {
      path: '/news/:id',
      name: 'SpecificNews',
      component: () => import('@/views/SpecificNewsView.vue'),
      props: true,
    },
    {
      path: '/general-assemblies/:id',
      name: 'SpecificGeneralAssembly',
      component: () => import('@/views/SpecificGeneralAssemblyView.vue'),
      props: true,
    },
    {
      path: '/contact',
      name: 'contact',
      component: () => import('@/views/ContactView.vue'),
    },
    {
      path: '/members',
      name: 'members',
      component: () => import('@/views/MembersView.vue'),
      meta: { requiresAuth: true },
    },
    {
      path: '/register',
      name: 'register',
      component: () => import('@/views/RegisterView.vue'),
      meta: { guest: true },
    },
    {
      path: '/forgot-password',
      name: 'forgot-password',
      component: () => import('@/views/ForgotPasswordView.vue'),
      meta: { guest: true },
    },
    {
      path: '/reset-password',
      name: 'reset-password',
      component: () => import('@/views/ResetPasswordView.vue'),
    },
    {
      path: '/member-portal',
      name: 'member-portal',
      component: () => import('@/views/MemberPortalView.vue'),
      meta: { requiresAuth: true },
    },
    {
      path: '/change-profile',
      name: 'change-profile',
      component: () => import('@/views/ChangeProfile.vue'),
      meta: { requiresAuth: true },
    },
    {
      path: '/confirm-change-email',
      name: 'confirm-change-email',
      component: () => import('@/views/ConfirmChangeEmailView.vue'),
    },
    {
      path: '/revoke-change-email',
      name: 'revoke-change-email',
      component: () => import('@/views/RevokeChangeEmailView.vue'),
    },
  ],
})

// Navigation guard for authentication
router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()
  const requiresAuth = to.matched.some((record) => record.meta.requiresAuth)
  const guestOnly = to.matched.some((record) => record.meta.guest)

  if (requiresAuth && !authStore.isAuthenticated) {
    // Redirect to login if route requires auth and user is not authenticated
    next({ name: 'login', query: { redirect: to.fullPath } })
  } else if (guestOnly && authStore.isAuthenticated) {
    // Redirect to home if route is for guests only and user is authenticated
    next({ name: 'home' })
  } else {
    next()
  }
})

export default router
