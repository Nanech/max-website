<script setup>
const props = defineProps({
  color: {
    type: String,
    default: 'black',
  },
})

const colorClasses = {
  black: 'text-black',
  white: 'text-white',
}
</script>

<template>
  <div :class="colorClasses[props.color]">
    <router-link to="/" class="transition-opacity hover:opacity-80">
      <b>Максим Горшков</b>
      <p>Независимый фотограф</p>
    </router-link>
  </div>
</template>
