<script setup>
import Header from '@/components/Header.vue'
import Footer from '@/components/Footer.vue'

const links = [
  { to: '/gallery', label: '/ Портфолио' },
  { to: '/price', label: '/ Прайс' },
  { to: '/about_me', label: '/ Обо мне' },
  { to: '/contacts', label: '/ Контакты' },
]

const socialMedia = [
  { to: 'https://t.me/withmax', label: '/ Телеграм' },
  { to: 'https://instagram.com/yourprofile', label: ' / ВК' },
  { to: 'https://facebook.com/yourprofile', label: '/ WhatsApp' },
]
</script>

<template>
  <div class="flex h-screen w-screen flex-col">
    <Header :links="links" />
    <main class="flex-grow font-mono">
      <slot />
    </main>
    <Footer :links="links" :socialMedia="socialMedia" />
  </div>
</template>
