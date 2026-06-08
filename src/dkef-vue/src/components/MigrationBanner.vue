<script setup lang="ts">
import { ref } from 'vue'

const DISMISS_KEY = 'migration-banner-dismissed'

const dismissed = ref(localStorage.getItem(DISMISS_KEY) === 'true')

function dismiss() {
  dismissed.value = true
  localStorage.setItem(DISMISS_KEY, 'true')
}
</script>

<template>
  <div
    v-if="!dismissed"
    class="px-4 py-4 bg-amber-600 text-navy-950"
  >
    <div class="max-w-4xl mx-auto flex items-center justify-between gap-4">
      <p class="text-sm leading-relaxed">
        Har du i forvejen en konto? Der er sket en overflytning af systemer. Brug
        <router-link to="/forgot-password" class="font-semibold underline hover:text-navy-800">
          glem adgangskode
        </router-link>
        for at nulstille din adgangskode.
      </p>

      <button
        @click="dismiss"
        class="flex-shrink-0 p-1 cursor-pointer text-inherit hover:text-navy-800 transition-colors"
        aria-label="Luk"
      >
        <svg
          xmlns="http://www.w3.org/2000/svg"
          class="h-5 w-5"
          viewBox="0 0 20 20"
          fill="currentColor"
        >
          <path
            d="M6.28 5.22a.75.75 0 00-1.06 1.06L8.94 10l-3.72 3.72a.75.75 0 101.06 1.06L10 11.06l3.72 3.72a.75.75 0 101.06-1.06L11.06 10l3.72-3.72a.75.75 0 00-1.06-1.06L10 8.94 6.28 5.22z"
          />
        </svg>
      </button>
    </div>
  </div>
</template>
