import { createRouter, createWebHistory } from 'vue-router'
import type { RouteRecordRaw } from 'vue-router' // <-- Импортируем тип маршрута

// Импорты страниц
import Contacts from '@/views/Contacts.vue'
import Gallery from '@/views/Gallery.vue'
import Home from '@/views/Home.vue'
import Price from '@/views/Price.vue'

// Явно указываем тип : RouteRecordRaw[]
const routes: RouteRecordRaw[] = [
  { path: '/', name: 'Home', component: Home },
  { path: '/gallery', name: 'Gallery', component: Gallery },
  { path: '/contacts', name: 'Contact', component: Contacts },
  { path: '/price', name: 'Price', component: Price },
  { path: '/:pathMatch(.*)*', redirect: '/' },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

export default router
