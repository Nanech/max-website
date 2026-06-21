<script lang="ts" setup>
import { TeamMember } from '@/types/credits'

defineProps<{
  member: TeamMember
}>()
</script>

<template>
  <div class="text-center">
    <h3 class="" text-7x1 font-bold mb-4>
      {{ member.name }} / <br />
      {{ member.role }}
    </h3>

    <br />

    <div class="mb-6">
      <h4 class="text-2x mb-2 text-black">СВЯЗАТЬСЯ:</h4>
      <ul class="space-y-1">
        <li v-for="link in member.contactLinks" :key="link.url">
          <a
            :href="link.url"
            target="_blank"
            rel="noopener noreferrer"
            class="text-sm text-gray-400"
          >
            {{ link.label }}
          </a>
        </li>
      </ul>
    </div>

    <div v-if="member.subscribeLinks && member.subscribeLinks.length > 0">
      <h4 class="text-2x mb-2 text-black">ПОДПИСАТЬСЯ:</h4>
      <ul class="space-y-1">
        <li v-for="link in member.subscribeLinks" :key="link.label">
          <a :href="link.url" target="_blank" class="text-xs text-gray-400">
            {{ link.label }}
          </a>
        </li>
      </ul>
    </div>
  </div>
</template>
