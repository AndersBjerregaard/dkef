<script setup lang="ts">
import { RouterLink } from 'vue-router'
import type { PublishedGeneralAssembly } from '@/types/generalAssembly'
import { computed } from 'vue'

const props = defineProps<{ publishedGeneralAssembly: PublishedGeneralAssembly }>()

const dateTime = computed(() => {
  const date = new Date(props.publishedGeneralAssembly?.dateTime)
  return new Intl.DateTimeFormat(undefined, {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: 'numeric',
    minute: 'numeric',
  }).format(date)
})
</script>

<template>
  <RouterLink :to="`/general-assemblies/${props.publishedGeneralAssembly.id}`">
    <div
      class="border border-gray-600 p-4 rounded-2xl max-w-sm flex flex-col cursor-pointer hover:bg-gray-600"
    >
      <div class="h-60 pb-4">
        <img
          class="picture"
          :src="props.publishedGeneralAssembly?.thumbnailUrl"
          alt="general assembly picture"
        />
      </div>
      <div class="text-content-area flex-grow flex flex-col">
        <div class="h-20 overflow-hidden">
          <span class="line-clamp-1">{{ props.publishedGeneralAssembly?.section }}</span>
        </div>
        <div class="h-20 overflow-hidden">
          <span class="line-clamp-2">{{ props.publishedGeneralAssembly?.title }}</span>
        </div>
        <div class="h-20 overflow-hidden">
          <span class="line-clamp-2">{{ props.publishedGeneralAssembly?.address }}</span>
        </div>
        <div class="h-10 overflow-hidden">
          <span class="line-clamp-1">{{ dateTime }}</span>
        </div>
      </div>
    </div>
  </RouterLink>
</template>

<style lang="css" scoped>
.picture {
  height: 100%;
  width: 100%;
  object-fit: fill;
}
</style>
