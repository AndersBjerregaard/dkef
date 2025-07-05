<script setup lang="ts">
import type { PublishedEvent } from '@/types/events';
import { computed } from 'vue';


const props = defineProps<{ publishedEvent: PublishedEvent }>();

const dateTime = computed(() => {

})

</script>

<template>
  <div class="border border-gray-600 p-4 rounded-2xl max-w-64">
    <div class="h-60 pb-4">
      <img class="picture"
        :src="props.publishedEvent?.thumbnailUrl"
        alt="event picture">
    </div>
    <div class="truncate">
      <div>
        <span>{{ props.publishedEvent?.section }}</span>
      </div>
      <div>
        <span>{{ props.publishedEvent?.title }}</span>
      </div>
      <div>
        <span>{{ props.publishedEvent?.address }}</span>
      </div>
    </div>
  </div>
</template>

<style lang="css" scoped>
.picture {
  height: 100%;
  width: 100%;
  object-fit: fill;
}
</style>
