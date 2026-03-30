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
  <RouterLink
    :to="`/general-assemblies/${props.publishedGeneralAssembly.id}`"
    class="block w-full h-full"
  >
    <div
      class="h-full min-h-[360px] border-2 border-theme-border rounded-2xl overflow-hidden flex flex-col cursor-pointer hover:border-amber-500 hover:shadow-[0_0_16px_2px_rgba(245,158,11,0.25)] bg-theme-mute transition-all duration-200 ease-out hover:scale-[1.02] will-change-transform"
    >
      <!-- Thumbnail -->
      <div class="h-44 shrink-0 bg-theme-soft">
        <img
          class="h-full w-full object-cover"
          :src="props.publishedGeneralAssembly?.thumbnailUrl"
          alt="Generalforsamling"
        />
      </div>

      <!-- Content -->
      <div class="flex flex-col flex-1 p-4 gap-2">
         <!-- Type badge and attachments count -->
         <div class="flex items-center justify-between">
           <span class="text-xs font-semibold uppercase tracking-wide text-theme-accent"
             >Generalforsamling</span
           >
           <span
             v-if="props.publishedGeneralAssembly?.attachments?.length"
             class="text-xs font-semibold bg-orange-600 text-white px-2 py-1 rounded"
           >
             📎 {{ props.publishedGeneralAssembly.attachments.length }}
           </span>
         </div>

        <!-- Section -->
        <p class="text-xs text-theme-text line-clamp-1">
          {{ props.publishedGeneralAssembly?.section }}
        </p>

        <!-- Title -->
        <p class="font-semibold line-clamp-2 leading-snug h-10 overflow-hidden">
          {{ props.publishedGeneralAssembly?.title }}
        </p>

        <!-- Metadata -->
        <div class="mt-auto flex flex-col gap-1 text-sm text-theme-text">
          <div class="flex items-start gap-1 h-10 overflow-hidden">
            <svg
              class="h-4 w-4 shrink-0 mt-0.5 text-theme-muted"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M17.657 16.657L13.414 20.9a2 2 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z"
              />
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M15 11a3 3 0 11-6 0 3 3 0 016 0z"
              />
            </svg>
            <span class="line-clamp-2">{{ props.publishedGeneralAssembly?.address }}</span>
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
            <span class="line-clamp-1">{{ dateTime }}</span>
          </div>
        </div>
      </div>
    </div>
  </RouterLink>
</template>
