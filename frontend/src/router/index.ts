import { createRouter, createWebHistory } from 'vue-router'
import type { RouteRecordRaw } from 'vue-router' // <-- Импортируем тип маршрута

// Импорты страниц
import AboutMe from '@/pages/AboutMe.vue'
import Contacts from '@/pages/Contacts.vue'
import Gallery from '@/pages/Gallery.vue'
import Home from '@/pages/Home.vue'
import Price from '@/pages/Price.vue'

// Явно указываем тип : RouteRecordRaw[]
const routes: RouteRecordRaw[] = [
  { path: '/', name: 'Home', component: Home },
  { path: '/gallery', name: 'Gallery', component: Gallery },
  { path: '/contacts', name: 'Contact', component: Contacts },
  { path: '/about_me', name: 'AboutMe', component: AboutMe },
  { path: '/price', name: 'Price', component: Price },
  { path: '/:pathMatch(.*)*', redirect: '/' },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

export default router
