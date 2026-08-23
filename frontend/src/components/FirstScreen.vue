<script setup lang="ts">
import Gallery from '@/views/Gallery.vue'
</script>

<template>
  <div
    class="flex flex-col items-center px-[10px] pb-[150px] font-mono uppercase md:flex-row md:px-15"
  >
    <div class="flex w-full flex-col pt-10 md:w-1/2 md:pt-45">
      <div>
        <p class="text-base">стильные и<br />естественные</p>

        <h1 class="py-4 text-5xl font-bold">Фотосессии в нижнем новгороде</h1>

        <p class="text-end text-base">Никакой<br />неловкости.</p>
      </div>

      <p class="text-primary-gray pt-20 text-justify text-sm font-normal md:pt-40">
        Фотография — это магия остановленного времени. Это когда в одном кадре могут жить смех,
        волнение, нежность и целый ураган эмоций. Я не просто нажимаю на кнопку — я ловлю свет,
        движение и настроение, чтобы сохранить их навсегда. Каждая съемка — как маленький праздник,
        где вы главный герой. Добро пожаловать в мир, где каждый момент достоин быть искусством.
      </p>
    </div>

    <div class="flex w-full justify-end md:w-1/2">
      <!-- here we place img -->
    </div>
  </div>

  <Gallery />
</template>
