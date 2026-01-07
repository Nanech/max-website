<script setup>
import CommonButton from '@/components/CommonButton.vue'
import LogoComponent from '@/components/LogoComponent.vue'
import ArrowIcon from '@/components/ArrowIcon.vue'
import NavMenu from '@/components/NavMenu.vue'

const props = defineProps({
  links: {
    type: Array,
    required: true,
  },
})
</script>

<template>
  <header class="flex justify-between align-top items-start font-mono font-normal uppercase md:px-15 md:pt-8">
    <LogoComponent />

    <NavMenu :links="props.links" />

    <CommonButton
      :label="'Написать в телеграмм'"
      type="external"
      url="https://t.me/withmax22"
      isDark
    >
      <ArrowIcon />
    </CommonButton>
  </header>
</template>
