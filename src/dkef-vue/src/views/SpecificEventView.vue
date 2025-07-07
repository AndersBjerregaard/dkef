<script setup lang="ts">
import { useEventStore } from '@/stores/eventStore';
import { type PublishedEvent } from '@/types/events';
import { computed, onMounted } from 'vue';

const props = defineProps({
  id: {
    type: String,
    required: true
  }
});

const eventStore = useEventStore();

const currentEvent = computed<PublishedEvent | undefined>(() => eventStore.getEventById(props.id));

const dateTime = computed(() => {
  const event = currentEvent.value;
  if (event === undefined) {
    return '';
  }
  const date: Date = new Date(event!.dateTime);
  return new Intl.DateTimeFormat(undefined, {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: 'numeric',
    minute: 'numeric',
  }).format(date);
});

async function loadEvent(id: string): Promise<void> {
  await eventStore.fetchEvent(id);
}

onMounted(async () => {
  await loadEvent(props.id);
})

</script>

<template>
  <div class="py-16 w-screen px-8 justify-items-center">
    <div class="w-[70%]">
      <!-- Loading -->
      <div v-if="eventStore.isFetching" class="flex justify-center items-center min-h-[200px]">
        <svg class="animate-spin h-10 w-10 text-blue-500" xmlns="http://www.w3.org/2000/svg" fill="none"
          viewBox="0 0 24 24">
          <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
          <path class="opacity-75" fill="currentColor"
            d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z">
          </path>
        </svg>
        <span class="ml-3 text-lg">Henter...</span>
      </div>

      <!-- Error -->
      <div v-else-if="eventStore.error">
        <p class="text-4xl">{{ eventStore.error }}</p>
      </div>

      <!-- Event -->
      <div v-else-if="currentEvent">
        <div class="text-4xl pb-8">
          <h1>{{ currentEvent?.title }}</h1>
        </div>
        <div class="text-xl">
          <div class="pb-4">
            <h2>{{ currentEvent.section }}</h2>
          </div>
          <div class="pb-4">
            <h2>{{ currentEvent.address}}</h2>
          </div>
          <div class="pb-4">
            <h2>{{ dateTime }}</h2>
          </div>
        </div>
        <div class="flex items-center justify-center h-full">
          <div class="h-60 w-60 pb-4 flex items-center justify-center">
            <img class="max-w-full max-h-full object-contain" :src="currentEvent.thumbnailUrl" alt="event picture">
          </div>
        </div>
        <div class="whitespace-pre-wrap">
          {{ currentEvent.description }}
        </div>
      </div>

      <!-- Not Found -->
      <div v-else>
        <p class="text-4xl">Ikke fundet 😟</p>
      </div>

    </div>
  </div>
</template>

<style lang="css" scoped>
</style>
