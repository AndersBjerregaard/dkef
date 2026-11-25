<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { getSiteImageSlot } from '@/content/siteImages'
import { useSiteImageStore } from '@/stores/siteImageStore'

const props = defineProps<{ group: string; imageSlot: string }>()
const store = useSiteImageStore()
const failed = ref(false)
const current = computed(() => store.value(props.group, props.imageSlot))
const definition = computed(() => getSiteImageSlot(props.group, props.imageSlot))
const image = computed(() =>
  failed.value ? getSiteImageSlot(props.group, props.imageSlot) : current.value,
)
watch(
  () => current.value.url,
  () => {
    failed.value = false
  },
)
onMounted(() => {
  void store.loadGroup(props.group).catch(() => {})
})
</script>

<template>
  <img
    :src="image.url"
    :alt="image.alt"
    :style="{ aspectRatio: definition.aspectRatio, objectFit: 'cover' }"
    @error="failed = true"
  />
</template>
