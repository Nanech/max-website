<script setup lang="ts">
defineProps<{
  isDark: boolean
}>()
</script>

<template>
  <div :class="isDark ? 'text-white' : 'text-black'">
    <router-link to="/" class="transition-opacity hover:opacity-80">
      <b>Максим Горшков</b>
      <p>Независимый фотограф</p>
    </router-link>
  </div>
</template>
