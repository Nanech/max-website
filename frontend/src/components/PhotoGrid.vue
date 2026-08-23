<script setup lang="ts">
import { onMounted } from 'vue'
import { useAlbum } from '@/composables/useAlbum'

const albumId = 'ece57508-1d93-41b2-a6b0-557d84d37c6f' // Replace with actual album ID
const { data, loading, error, fetchAlbum } = useAlbum(albumId)

onMounted(() => {
  fetchAlbum()
})
</script>

<template>
  <div class="gallery w-full px-4 py-8">
    <!-- 1. Состояние загрузки -->
    <div v-if="loading" class="py-12 text-center font-mono text-lg">Загрузка галереи...</div>

    <!-- 2. Ошибка -->
    <div v-else-if="error" class="py-12 text-center font-mono text-red-500">
      Ошибка: {{ error }}
    </div>

    <!-- 3. Сетка фотографий (срабатывает, когда photosUrls пришел и он не пуст) -->
    <div
      v-else-if="data && data.photosUrls && data.photosUrls.length > 0"
      class="grid grid-cols-1 gap-6 sm:grid-cols-2 md:grid-cols-3"
    >
      <div
        v-for="(photo, index) in data.photosUrls"
        :key="index"
        class="group relative overflow-hidden rounded-lg bg-gray-100 shadow-sm transition-transform duration-300 hover:-translate-y-1 hover:shadow-md"
      >
        <!-- Берем именно photo.previewUrl из твоего DTO -->
        <img
          :src="photo.previewUrl"
          alt="Фотография"
          class="h-72 w-full object-cover transition-opacity duration-300 group-hover:opacity-90"
          loading="lazy"
        />
      </div>
    </div>

    <!-- 4. Пустой альбом -->
    <div v-else class="py-12 text-center font-mono text-gray-400">
      В этом альбоме пока нет фотографий.
    </div>
  </div>
</template>
