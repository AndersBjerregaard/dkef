<script setup lang="ts">
import { ref } from 'vue';
import EventComponent from './EventComponent.vue';
import {
  Menu, MenuButton, MenuItems, MenuItem,
  TransitionRoot, TransitionChild,
  Dialog, DialogPanel, DialogTitle
} from '@headlessui/vue'

// Example items
const items = ref([1, 2, 3]);

const isOpen = ref(false);

function closeModal() {
  isOpen.value = false;
}

function openModal() {
  isOpen.value = true;
}

</script>

<template>
  <div class="py-24">
    <div class="flex justify-center items-center">
      <h1 class="text-4xl">Arrangementer og Nyheder</h1>
    </div>
    <div class="flex justify-center items-center py-12 gap-x-8">
      <button
        class="flex justify-center rounded bg-gray-600 h-10 sm:h-12 py-2 w-24 sm:w-32 cursor-pointer hover:bg-gray-800 sm:text-lg">Alle</button>
      <button
        class="flex justify-center rounded bg-gray-600 h-10 sm:h-12 py-2 w-24 sm:w-32 cursor-pointer hover:bg-gray-800 sm:text-lg">Nyheder</button>
      <button
        class="flex justify-center rounded bg-gray-600 h-10 sm:h-12 py-2 w-24 sm:w-36 cursor-pointer hover:bg-gray-800 sm:text-lg">Arrangementer</button>
      <button
        class="flex justify-center rounded bg-gray-600 h-10 sm:h-12 py-2 w-24 sm:w-48 cursor-pointer hover:bg-gray-800 sm:text-lg">Generalforsamlinger</button>
    </div>
    <div class="flex justify-center items-center gap-x-8">
      <div v-for="x in items" :key="x">
        <EventComponent />
      </div>
    </div>
    <div class="flex justify-center items-center py-12 gap-x-8">
      <button class="flex justify-center rounded bg-gray-600 h-10 sm:h-12 py-2
        w-24 sm:w-32 cursor-pointer hover:bg-gray-800 sm:text-lg" @click="openModal">
        Ny Nyhed
      </button>
    </div>
    <TransitionRoot appear :show="isOpen" as="template">
      <Dialog as="div" @close="closeModal" class="relative z-10">
        <TransitionChild as="template" enter="duration-300 ease-out" enter-from="opacity-0" enter-to="opacity-100"
          leave="duration-200 ease-in" leave-from="opacity-100" leave-to="opacity-0">
          <div class="fixed inset-0 bg-black/25"></div>
        </TransitionChild>

        <div class="fixed inset-0 overflow-y-auto">
          <div class="flex min-h-full items-center justify-center p-4 text-center">
            <TransitionChild as="template" enter="duration-300 ease-out" enter-from="opacity-0 scale-95"
              enter-to="opacity-100 scale-100" leave="duration-200 ease-in" leave-from="opacity-100 scale-100"
              leave-to="opacity-0 scale-95">
              <DialogPanel
                class="w-full max-w-md transform overflow-hidden rounded-2xl bg-gray-700 p-6 text-left align-middle shadow-xl transition-all **origin-center** translate-z-0">

                <button type="button" class="absolute top-3 right-3 text-gray-400 hover:text-gray-500"
                  @click="closeModal">
                  <span class="sr-only">Close</span>
                  <svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor" aria-hidden="true">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                  </svg>
                </button>

                <DialogTitle as="h3" class="text-lg font-medium leading-6">
                  Ny Nyhed
                </DialogTitle>
                <div class="mt-2 py-2">
                  <p class="text-sm">
                    Du er nu logget på.
                  </p>
                </div>

                <div class="mt-4">
                  <button type="button"
                    class="inline-flex justify-center rounded-md border border-transparent bg-blue-100 px-4 py-2 text-sm font-medium text-blue-900 hover:bg-blue-200 focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 focus-visible:ring-offset-2 cursor-pointer"
                    @click="closeModal">
                    Fortsæt til DKEF
                  </button>
                </div>
              </DialogPanel>
            </TransitionChild>
          </div>
        </div>
      </Dialog>
    </TransitionRoot>
  </div>
</template>
