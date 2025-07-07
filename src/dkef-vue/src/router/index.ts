import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '@/views/HomeView.vue'

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
      props: true
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
    },
  ],
})

export default router
