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
  <RouterLink :to="`/news/${props.publishedNews.id}`" class="block w-full">
    <div
      class="h-full border border-theme-border rounded-2xl overflow-hidden flex flex-col cursor-pointer hover:border-amber-500/40 bg-theme-mute transition-colors"
    >
      <!-- Thumbnail -->
      <div class="h-44 shrink-0 bg-theme-soft">
        <img
          v-if="props.publishedNews?.thumbnailUrl"
          class="h-full w-full object-cover"
          :src="props.publishedNews?.thumbnailUrl"
          alt="news picture"
        />
        <div v-else class="h-full w-full flex items-center justify-center">
          <span class="text-theme-muted text-sm">Ingen billede</span>
        </div>
      </div>

      <!-- Content -->
      <div class="flex flex-col flex-1 p-4 gap-2">
        <!-- Type badge -->
        <span class="text-xs font-semibold uppercase tracking-wide text-emerald-400">Nyhed</span>

        <!-- Section -->
        <p class="text-xs text-theme-text line-clamp-1">{{ props.publishedNews?.section }}</p>

        <!-- Title -->
        <p class="font-semibold line-clamp-2 leading-snug h-10 overflow-hidden">
          {{ props.publishedNews?.title }}
        </p>

        <!-- Metadata -->
        <div class="mt-auto flex flex-col gap-1 text-sm text-theme-text">
          <div class="flex items-center gap-1 h-10 overflow-hidden">
            <svg
              class="h-4 w-4 shrink-0 text-theme-muted"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"
              />
            </svg>
            <span class="line-clamp-1">{{ props.publishedNews?.author }}</span>
          </div>
          <div class="flex items-center gap-1">
            <svg
              class="h-4 w-4 shrink-0 text-theme-muted"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
              />
            </svg>
            <span class="line-clamp-1">{{ publishedAt }}</span>
          </div>
        </div>
      </div>
    </div>
  </RouterLink>
</template>
