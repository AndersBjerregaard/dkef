<script setup lang="ts">

const props = defineProps<{ header?: string }>()

</script>

<template>
  <div class="border-x-2 border-gray-800 w-full py-1">
    <h2 class="text-lg overflow-ellipsis">{{ props.header }}</h2>
  </div>
</template>

<style lang="css" scoped></style>
