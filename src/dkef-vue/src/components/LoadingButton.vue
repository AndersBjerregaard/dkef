<script setup lang="ts">
  const props = defineProps({
    defaultText: {
      type: String,
      required: true
    },
    loadingText: {
      type: String,
      requried: true
    },
    isLoading: {
      type: Boolean,
      default: false
    },
  });

  const emit = defineEmits(['loadingButtonClick']);

  function handleClick() {
    if (!props.isLoading) {
      emit('loadingButtonClick');
    }
  };
</script>

<template>
  <div class="mt-4">
    <button
      class="inline-flex justify-center rounded-md border border-transparent bg-gray-300 px-4 py-2 text-md font-medium focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 text-gray-900 focus-visible:ring-offset-2"
      :disabled="isLoading"
      :class="{ 'cursor-pointer': !isLoading, 'hover:bg-gray-400': !isLoading }"
      @click="handleClick"
    >
      <span v-if="isLoading" class="flex items-center">
        {{ loadingText }}
        <svg class="animate-spin -ml-1 mr-3 h-5 w-5 text-gray-900" xmlns="http://www.w3.org/2000/svg" fill="none"
          viewBox="0 0 24 24">
          <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4">
          </circle>
          <path class="opacity-75" fill="currentColor"
            d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z">
          </path>
        </svg>
      </span>
      <span v-else>
        {{ defaultText }}
      </span>
    </button>
  </div>
</template>

<style lang="css" scoped>
@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}
.animate-spin {
  animation: spin 1s linear infinite;
}
</style>
