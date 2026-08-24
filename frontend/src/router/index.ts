import { createRouter, createWebHistory } from 'vue-router'
import type { RouteRecordRaw } from 'vue-router'

// Импорты страниц
import Contacts from '@/views/Contacts.vue'
import Gallery from '@/views/Gallery.vue'
import Home from '@/views/Home.vue'
import Price from '@/views/Price.vue'

// Явно указываем тип : RouteRecordRaw[]
const routes: RouteRecordRaw[] = [
  { path: '/', name: 'Home', component: Home, meta: { isDark: false } },
  { path: '/gallery', name: 'Gallery', component: Gallery, meta: { isDark: false } },
  { path: '/contacts', name: 'Contact', component: Contacts, meta: { isDark: false } },
  { path: '/price', name: 'Price', component: Price, meta: { isDark: true } },
  { path: '/:pathMatch(.*)*', redirect: '/' },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

export default router
