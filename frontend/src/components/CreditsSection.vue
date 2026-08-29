<script setup lang="ts">
import { ref } from 'vue'
import type { TeamMember } from '@/types/credits'
import MemberCard from './MemberCard.vue'

const team = ref<TeamMember[]>([
  {
    id: '1',
    name: 'ДАША ДЁМИНА',
    role: 'ВЕБ-ДИЗАЙНЕР',
    contactLinks: [
      { label: 'ТЕЛЕГРАМ', url: 'https://t.me/...' },
      { label: 'ВК', url: 'https://vk.com/...' },
    ],
  },
  {
    id: '2',
    name: 'МАКСИМ ГОРШКОВ',
    role: 'ФОТОГРАФ',
    contactLinks: [
      { label: 'ТЕЛЕГРАМ', url: 'https://t.me/...' },
      { label: 'ВК', url: 'https://vk.com/...' },
      { label: 'WHATSAPP', url: 'https://wa.me/...' },
    ],
    subscribeLinks: [
      { label: 'ТЕЛЕГРАМ', url: 'https://t.me/...' },
      { label: 'ВК', url: 'https://vk.com/...' },
    ],
  },
  {
    id: '3',
    name: 'НИКИТА АГЕЕВ',
    role: 'РАЗРАБОТЧИК',
    contactLinks: [
      { label: 'ТЕЛЕГРАМ', url: 'https://t.me/...' },
      { label: 'ВК', url: 'https://vk.com/...' },
    ],
  },
])
</script>

<template>
  <section class="max-auto w-full px-4 py-16 md:px-20">
    <h1 class="text-justify text-6xl uppercase md:text-9xl">Контакты /</h1>

    <div class="bg-white py-16">
      <div class="mx-auto grid max-w-7xl grid-cols-1 items-start gap-12 md:grid-cols-3 md:gap-6">
        <MemberCard v-for="member in team" :key="member.id" :member="member" />
      </div>
    </div>
  </section>
</template>
