<script setup>
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
  <footer class="flex-row items-center justify-between bg-black px-15 py-10 font-mono text-white">
    <div>
      <p class="text-5xl italic">( Свет в плену <br />момента )</p>

      <div class="flex items-start justify-between space-x-6 pt-20 text-sm">
        <LogoComponent color="white" />

        <div class="flex justify-between align-top">
          <NavMenu :links="props.links" />

          <NavMenu :links="props.socialMedia" />
        </div>

        <CommonButton
          :label="'Написать в телеграмм'"
          type="external"
          url="https://t.me/withmax22"
          isDark
        >
          <ArrowIcon />
        </CommonButton>
      </div>
    </div>
  </footer>
</template>
