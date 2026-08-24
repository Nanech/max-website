<script setup lang="ts">
import type { PriceCard as PriceCardType } from '@/types/priceCard'
import PriceCard from '@/components/PriceCard.vue'

defineProps<{
  priceCards: PriceCardType[]
}>()
</script>

<template>
  <section class="w-full bg-black px-4 py-16 text-white md:py-24">
    <div class="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
      <!-- Заголовок -->
      <div class="mb-12 flex items-center justify-between">
        <h2 class="font-mono text-5xl font-bold tracking-wider uppercase md:text-4xl">Прайс /</h2>
      </div>

      <div>
        <div v-if="priceCards.length === 0" class="text-center font-mono text-lg">
          Прайс пока пуст.
        </div>

        <div v-else class="grid grid-cols-1 gap-6 md:grid-cols-2 lg:gap-8">
          <PriceCard v-for="card in priceCards" :key="card.id" :priceCard="card" />
        </div>
      </div>
    </div>
  </section>
</template>
