<script setup lang="ts">
import { computed } from 'vue'
import { useNavMenu } from '@/composables/useNavMenu'

const props = defineProps({
  links: {
    type: Array,
    required: true,
  },
  direction: {
    type: String,
    default: 'row',
    validator: (value) => ['row', 'column'].includes(value),
  },
})

const { menuItems } = useNavMenu(props.links)

const computedClasses = computed(() => {
  const baseClasses = 'list-none'
  const directionClasses = props.direction === 'row' ? 'flex-row space-x-6' : 'flex-col space-y-2'

  return `${baseClasses} ${directionClasses}`
})
</script>

<template>
  <ul :class="computedClasses">
    <li v-for="link in menuItems" :key="link.to" class="uppercase">
      <router-link
        v-if="!link.external"
        :to="link.to"
        class="transition-colors duration-300 hover:text-blue-500"
        active-class="text-blue-600 font-semibold"
      >
        {{ link.label }}
      </router-link>

      <a
        v-else
        :href="link.to"
        target="_blank"
        rel="noopener noreferrer"
        class="transition-colors duration-300 hover:text-blue-500"
      >
        {{ link.label }}
      </a>
    </li>
  </ul>
</template>
