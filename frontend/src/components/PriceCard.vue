<script setup lang="ts">
import type { PriceCard } from '@/types/priceCard'

defineProps<{
  priceCard: PriceCard
}>()

const formatPrice = (val: number) => new Intl.NumberFormat('ru-RU').format(val)
</script>

<template>
  <div class="flex flex-col justify-between bg-white p-6 text-black shadow-md">
    <!-- Image Container -->
    <div class="mb-6 aspect-[12/9] overflow-hidden bg-gray-100">
      <img
        :src="priceCard.thumbnailUrl"
        :alt="priceCard.categoryName"
        class="h-full w-full object-cover grayscale"
        loading="lazy"
      />
    </div>

    <!-- Название категории -->
    <h3 class="mb-6 font-mono text-xl font-bold tracking-wide uppercase">
      {{ priceCard.categoryName }}
    </h3>

    <!-- Описание -->
    <div class="space-y-2 font-mono text-xs tracking-tight text-gray-700 uppercase">
      <p>
        Длительность:
        <span class="text-bold font-bold">{{ priceCard.durationInHours }} час(а)</span>
      </p>
      <p>
        Фотографий в ретуши:
        <span class="text-bold font-bold">{{ priceCard.numberOfPhotos }} штук</span>
      </p>
      <p>
        Срок обработки:
        <span class="text-bold font-bold">{{ priceCard.orderProcessingTimeInDays }} дней</span>
      </p>
    </div>

    <!-- Цена -->
    <div class="mt-8 pt-6">
      <div class="flex items-baseline justify-between font-mono">
        <span class="text-3xl font-bold uppercase">Стоимость:</span>
        <span class="text-2xl font-bold">{{ formatPrice(priceCard.price) }} ₽</span>
      </div>
    </div>
  </div>
</template>
