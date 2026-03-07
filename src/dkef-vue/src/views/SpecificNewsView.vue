<script setup lang="ts">
import { useNewsStore } from '@/stores/newsStore'
import type { PublishedNews } from '@/types/news'
import { computed, onMounted } from 'vue'

const props = defineProps({
  id: {
    type: String,
    required: true,
  },
})

const newsStore = useNewsStore()

const currentNews = computed<PublishedNews | undefined>(() => newsStore.getNewsById(props.id))

const publishedAt = computed(() => {
  const item = currentNews.value
  if (item === undefined) return ''
  const date = new Date(item.publishedAt)
  return new Intl.DateTimeFormat(undefined, {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  }).format(date)
})

onMounted(async () => {
  await newsStore.fetchNewsItem(props.id)
})
</script>

<template>
  <div class="py-16 w-screen px-8 justify-items-center">
    <div class="w-[70%]">
      <div class="pb-4">
        <RouterLink
          to="/events-and-news"
          class="flex justify-center items-center rounded bg-gray-600 h-14 w-64 p-2 cursor-pointer hover:bg-gray-800"
        >
          &larr; Tilbage til arrangementer og nyheder
        </RouterLink>
      </div>

      <!-- Loading -->
      <div v-if="newsStore.isFetching" class="flex justify-center items-center min-h-[200px]">
        <svg
          class="animate-spin h-10 w-10 text-blue-500"
          xmlns="http://www.w3.org/2000/svg"
          fill="none"
          viewBox="0 0 24 24"
        >
          <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
          <path
            class="opacity-75"
            fill="currentColor"
            d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
          ></path>
        </svg>
        <span class="ml-3 text-lg">Henter...</span>
      </div>

      <!-- Error -->
      <div v-else-if="newsStore.error">
        <p class="text-4xl">{{ newsStore.error }}</p>
      </div>

      <!-- News item -->
      <div v-else-if="currentNews">
        <div class="text-4xl pb-8">
          <h1>{{ currentNews.title }}</h1>
        </div>
        <div class="text-xl">
          <div v-if="currentNews.section" class="pb-4">
            <h2>{{ currentNews.section }}</h2>
          </div>
          <div v-if="currentNews.author" class="pb-4">
            <h2>{{ currentNews.author }}</h2>
          </div>
          <div class="pb-4">
            <h2>{{ publishedAt }}</h2>
          </div>
        </div>
        <div v-if="currentNews.thumbnailUrl" class="flex items-center justify-center h-full">
          <div class="h-60 w-60 pb-4 flex items-center justify-center">
            <img
              class="max-w-full max-h-full object-contain"
              :src="currentNews.thumbnailUrl"
              alt="news picture"
            />
          </div>
        </div>
        <div class="whitespace-pre-wrap">
          {{ currentNews.description }}
        </div>
      </div>

      <!-- Not Found -->
      <div v-else>
        <p class="text-4xl">Ikke fundet</p>
      </div>
    </div>
  </div>
</template>

<style lang="css" scoped></style>
