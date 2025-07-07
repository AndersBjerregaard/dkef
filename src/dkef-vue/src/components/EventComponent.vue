<script setup lang="ts">
import { RouterLink } from 'vue-router';
import type { PublishedEvent } from '@/types/events';
import { computed } from 'vue';

const props = defineProps<{ publishedEvent: PublishedEvent }>();

const dateTime = computed(() => {
  const date = new Date(props.publishedEvent?.dateTime);

  return new Intl.DateTimeFormat(undefined, {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: 'numeric',
    minute: 'numeric',
  }).format(date);
})
</script>

<template>
  <RouterLink :to="`/events-and-news/${props.publishedEvent.id}`">
    <div class="border border-gray-600 p-4 rounded-2xl max-w-sm flex flex-col cursor-pointer hover:bg-gray-600">
      <div class="h-60 pb-4">
        <img class="picture" :src="props.publishedEvent?.thumbnailUrl" alt="event picture">
      </div>
      <div class="text-content-area flex-grow">
        <div>
          <span>{{ props.publishedEvent?.section }}</span>
        </div>
        <div class="py-4">
          <span>{{ props.publishedEvent?.title }}</span>
        </div>
        <div class="pb-4">
          <span>{{ props.publishedEvent?.address }}</span>
        </div>
        <div>
          <span>{{ dateTime }}</span>
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

.text-content-area {
  /* Calculate a suitable height based on your design for two lines of text
     for each item + padding. This will require some trial and error. */
  min-height: calc(var(--spacing) * 48); /* Adjust this value as needed */
  overflow: hidden;
}
</style>
