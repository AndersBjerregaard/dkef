<script setup lang="ts">
import { RouterLink } from 'vue-router'
import type { PublishedNews } from '@/types/news'
import { computed } from 'vue'

const props = defineProps<{ publishedNews: PublishedNews }>()

const publishedAt = computed(() => {
  const date = new Date(props.publishedNews?.publishedAt)
  return new Intl.DateTimeFormat(undefined, {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  }).format(date)
})
</script>

<template>
  <RouterLink :to="`/news/${props.publishedNews.id}`">
    <div
      class="border border-gray-600 p-4 rounded-2xl max-w-sm flex flex-col cursor-pointer hover:bg-gray-600"
    >
      <div class="h-60 pb-4">
        <img
          v-if="props.publishedNews?.thumbnailUrl"
          class="picture"
          :src="props.publishedNews?.thumbnailUrl"
          alt="news picture"
        />
        <div v-else class="h-full w-full flex items-center justify-center bg-gray-700 rounded-xl">
          <span class="text-gray-400 text-sm">Ingen billede</span>
        </div>
      </div>
      <div class="text-content-area flex-grow flex flex-col">
        <div class="h-10 overflow-hidden">
          <span class="line-clamp-1 text-sm text-gray-400">{{ props.publishedNews?.section }}</span>
        </div>
        <div class="h-20 overflow-hidden">
          <span class="line-clamp-2 font-semibold">{{ props.publishedNews?.title }}</span>
        </div>
        <div class="h-10 overflow-hidden">
          <span class="line-clamp-1 text-sm text-gray-400">{{ props.publishedNews?.author }}</span>
        </div>
        <div class="h-10 overflow-hidden">
          <span class="line-clamp-1 text-sm">{{ publishedAt }}</span>
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
