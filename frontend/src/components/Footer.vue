<script setup lang="ts">
import LogoComponent from '@/components/LogoComponent.vue'
import NavMenu from '@/components/NavMenu.vue'
import CommonButton from '@/components/CommonButton.vue'
import ArrowIcon from '@/components/ArrowIcon.vue'

const props = defineProps({
  links: {
    type: Array,
    required: true,
  },
  socialMedia: {
    type: Array,
    required: true,
  },
})
</script>

<template>
  <footer class="w-full bg-black px-4 py-12 text-white md:py-16">
    <div class="mx-auto flex flex-col gap-20 lg:justify-between">
      <p class="text-5xl italic md:text-6xl lg:text-7xl">
        ( Свет в плену <br class="hidden sm:inline" />момента )
      </p>

      <div class="flex flex-col gap-8 lg:grid lg:grid-cols-3 lg:items-start lg:gap-4">
        <div class="flex justify-start">
          <LogoComponent :isDark="true" />
        </div>

        <div class="flex flex-row gap-6 sm:items-start lg:justify-center lg:gap-12">
          <NavMenu :links="props.links" />
          <NavMenu :links="props.socialMedia" />
        </div>

        <div class="flex justify-start lg:justify-end">
          <CommonButton
            :label="'Написать в телеграмм'"
            type="external"
            url="https://t.me/withmax22"
            :isDark="true"
          >
            <ArrowIcon />
          </CommonButton>
        </div>
      </div>
    </div>
  </footer>
</template>
