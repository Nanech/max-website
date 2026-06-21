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
  <div class="border-t border-gray-100 bg-white px-6 py-16 md:px-15">
    <div class="mx-auto grid max-w-7xl grid-cols-1 items-start gap-12 md:grid-cols-3 md:gap-6">
      <MemberCard v-for="member in team" :key="member.id" :member="member" />
    </div>
  </div>
</template>
