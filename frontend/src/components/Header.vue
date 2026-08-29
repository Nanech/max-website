<script setup lang="ts">
import { useRoute } from 'vue-router'
import { computed } from 'vue'

import CommonButton from '@/components/CommonButton.vue'
import LogoComponent from '@/components/LogoComponent.vue'
import ArrowIcon from '@/components/ArrowIcon.vue'
import NavMenu from '@/components/NavMenu.vue'

interface Link {
  name: string
  path: string
}

const props = defineProps<{
  links: Link[]
}>()

const route = useRoute()

const isDark = computed(() => Boolean(route.meta.isDark))
</script>

<template>
  <header
    class="w-full transition-colors duration-300"
    :class="isDark ? 'bg-black text-white' : 'bg-white text-black'"
  >
    <div
      class="mx-auto flex max-w-7xl items-start justify-between px-4 pt-6 font-mono uppercase sm:px-8 md:pt-8"
    >
      <LogoComponent :isDark="isDark" />

      <NavMenu :links="props.links" />

      <CommonButton
        :label="'Написать в телеграмм'"
        type="external"
        url="https://t.me/withmax22"
        :isDark="true"
      >
        <ArrowIcon />
      </CommonButton>
    </div>
  </header>
</template>
