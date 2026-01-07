import { createRouter, createWebHistory } from 'vue-router'

// Импорты страниц
import AboutMe from '@/pages/AboutMe.vue'
import Contacts from '@/pages/Contacts.vue'
import Gallery from '@/pages/Gallery.vue'
import Home from '@/pages/Home.vue'
import Price from '@/pages/Price.vue'

// Маршруты
const routes = [
  { path: '/', name: 'Home', component: Home },
  { path: '/gallery', name: 'Gallery', component: Gallery },
  { path: '/contacts', name: 'Contact', component: Contacts },
  { path: '/about_me', name: 'AboutMe', component: AboutMe },
  { path: '/price', name: 'Price', component: Price },
  { path: '/:pathMatch(.*)*', redirect: '/' }, // на всякий случай редирект на Home
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

export default router
