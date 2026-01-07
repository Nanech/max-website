<script setup>
import { computed } from 'vue'

const props = defineProps({
  label: {
    type: String,
    required: true,
  },
  type: {
    type: String,
    default: 'button',
    validator: (value) => ['button', 'link', 'external'].includes(value),
  },
  url: {
    type: String,
    default: '',
  },
  isDark: {
    type: Boolean,
    default: false,
  },
  isActive: {
    type: Boolean,
    default: false,
  },
})

const baseClasses = 'uppercase font-bold px-4 py-2 transition-all duration-300'
const computedClasses = computed(() => {
  const isLightButton = (props.isDark && !props.isActive) || (!props.isDark && props.isActive)

  let classes = baseClasses

  if (isLightButton) {
    classes += ' bg-white text-black border-black hover:bg-black hover:text-white'
  } else {
    classes += ' bg-primary-black text-white border-black hover:bg-white hover:text-black'
  }

  return classes
})

const getComponent = computed(() => {
  switch (props.type) {
    case 'link':
      return 'router-link'
    case 'external':
      return 'a'
    default:
      return 'button'
  }
})

const getComponentProps = computed(() => {
  switch (props.type) {
    case 'link':
      return { to: props.url }
    case 'external':
      return {
        href: props.url,
        target: '_blank',
        rel: 'noopener noreferrer',
      }
    default:
      return { type: 'button' }
  }
})
</script>

<template>
  <component :is="getComponent" :class="computedClasses" v-bind="getComponentProps">
    <div class="flex items-center justify-between gap-2 h-4">
      <span>{{ label }}</span>
      <slot></slot>
    </div>
  </component>
</template>
