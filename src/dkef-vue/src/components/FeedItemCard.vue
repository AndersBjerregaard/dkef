<script setup lang="ts">
import EventComponent from '@/components/EventComponent.vue'
import GeneralAssemblyComponent from '@/components/GeneralAssemblyComponent.vue'
import NewsComponent from '@/components/NewsComponent.vue'
import type { FeedItem } from '@/types/feed'

defineProps<{ item: FeedItem }>()
</script>

<template>
  <EventComponent
    v-if="item.kind === 'event'"
    :published-event="{
      id: item.id,
      title: item.title,
      section: item.section,
      address: item.address ?? '',
      dateTime: item.dateTime ?? '',
      description: item.description,
      thumbnailUrl: item.thumbnailUrl,
      attachmentUrls: item.attachmentUrls ?? [],
      createdAt: item.createdAt,
    }"
  />

  <NewsComponent
    v-else-if="item.kind === 'news'"
    :published-news="{
      id: item.id,
      title: item.title,
      section: item.section,
      description: item.description,
      thumbnailUrl: item.thumbnailUrl,
      attachmentUrls: item.attachmentUrls ?? [],
      dateTime: item.dateTime ?? '',
      createdAt: item.createdAt,
    }"
  />

  <GeneralAssemblyComponent
    v-else
    :published-general-assembly="{
      id: item.id,
      title: item.title,
      section: item.section,
      address: item.address ?? '',
      dateTime: item.dateTime ?? '',
      description: item.description,
      thumbnailUrl: item.thumbnailUrl,
      attachmentUrls: item.attachmentUrls ?? [],
      createdAt: item.createdAt,
    }"
  />
</template>
