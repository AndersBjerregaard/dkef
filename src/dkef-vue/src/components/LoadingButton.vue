<script setup lang="ts">
const props = defineProps({
  defaultText: {
    type: String,
    required: true,
  },
  loadingText: {
    type: String,
    requried: true,
  },
  isLoading: {
    type: Boolean,
    default: false,
  },
  type: {
    type: String as () => 'button' | 'submit' | 'reset',
    default: 'button',
  },
})

const emit = defineEmits(['loadingButtonClick'])

function handleClick() {
  if (!props.isLoading) {
    emit('loadingButtonClick')
  }
}
</script>

<template>
  <div class="mt-4">
    <button
      class="inline-flex justify-center rounded-md border border-transparent bg-amber-600 text-navy-950 px-4 py-2 font-semibold focus:outline-none focus-visible:ring-2 focus-visible:ring-theme-accent focus-visible:ring-offset-2 shadow-lg shadow-amber-600/20 transition-colors"
      :type="type"
      :disabled="isLoading"
      :class="{
        'cursor-pointer hover:bg-amber-500': !isLoading,
        'cursor-not-allowed opacity-70': isLoading,
      }"
      @click="type === 'button' ? handleClick() : undefined"
    >
      <span v-if="isLoading" class="flex items-center">
        {{ loadingText }}
        <svg
          class="animate-spin ml-2 h-5 w-5 text-navy-950"
          xmlns="http://www.w3.org/2000/svg"
          fill="none"
          viewBox="0 0 24 24"
        >
          <circle
            class="opacity-25"
            cx="12"
            cy="12"
            r="10"
            stroke="currentColor"
            stroke-width="4"
          ></circle>
          <path
            class="opacity-75"
            fill="currentColor"
            d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
          ></path>
        </svg>
      </span>
      <span v-else>
        {{ defaultText }}
      </span>
    </button>
  </div>
</template>
