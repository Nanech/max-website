<script setup lang="ts">
import PriceSection from '@/components/PriceSection.vue'
import type { PriceCard } from '@/types/priceCard'

const priceList: PriceCard[] = [
  {
    id: 1,
    thumbnailUrl: 'idk',
    categoryName: 'Персональная фотосессия',
    durationInHours: 1,
    numberOfPhotos: 20,
    orderProcessingTimeInDays: 3,
    price: 3000,
  },
  {
    id: 2,
    thumbnailUrl: 'idk',
    categoryName: 'Love story',
    durationInHours: 2,
    numberOfPhotos: 35,
    orderProcessingTimeInDays: 6,
    price: 5000,
  },
  {
    id: 3,
    thumbnailUrl: 'idk',
    categoryName: 'Репортаж',
    durationInHours: 3,
    numberOfPhotos: 40,
    orderProcessingTimeInDays: 7,
    price: 8000,
  },
  {
    id: 4,
    thumbnailUrl: 'idk',
    categoryName: 'Свадебная съемка',
    durationInHours: 3,
    numberOfPhotos: 40,
    orderProcessingTimeInDays: 8,
    price: 15000,
  },
]
</script>

<template>
  <PriceSection :priceCards="priceList" />
</template>
