<script setup lang="ts">
import { ref, type Ref } from 'vue';
import EventComponent from './EventComponent.vue';
import {
  TransitionRoot, TransitionChild,
  Dialog, DialogPanel, DialogTitle
} from '@headlessui/vue'
import { v4 as uuidv4 } from 'uuid'
import apiservice from '@/services/apiservice';
import urlservice from '@/services/urlservice';
import type { AxiosResponse } from 'axios';
import axios from 'axios';

// Example items
const items = ref([1, 2, 3]);

const isOpen: Ref<boolean> = ref(false);
const isLoading: Ref<boolean> = ref(false); // New ref for loading state

let newsTitle: Ref<string> = ref(''); // Use ref for newsTitle
let newsFile: Ref<File | null> = ref(null); // Use ref for newsFile

let fileUploadError: Ref<boolean> = ref(false);
let submitError: Ref<boolean> = ref(false);

function closeModal() {
  // Reset fields
  newsTitle.value = '';
  newsFile.value = null;

  isOpen.value = false;
}

function openModal() {
  // Reset fields
  newsTitle.value = '';
  newsFile.value = null;

  isOpen.value = true;
  submitError.value = false; // Reset submit error on opening
  fileUploadError.value = false; // Reset file upload error on opening
}

function handleNewsTitleChange(event: Event) {
  submitError.value = false;
}

function handleFileUpload(event: Event) {
  submitError.value = false;
  fileUploadError.value = false;
  const input = event.target as HTMLInputElement;
  if (input.files && input.files.length > 0) {
    newsFile.value = input.files[0];
    if (newsFile.value.size === 0) {
      fileUploadError.value = true;
    }
  } else {
    newsFile.value = null; // Clear if no file is selected (e.g. user cancels)
  }
}

async function createNews() {
  submitError.value = false;
  if (newsFile.value === null) {
    submitError.value = true;
    return;
  }
  if (newsTitle.value === '') {
    submitError.value = true;
    return;
  }

  if (!submitError.value) {
    isLoading.value = true; // Set loading to true
    try {
      const guid: string = uuidv4();
      const response: AxiosResponse<string> = await apiservice.get<string>(urlservice.getEventPresignedUrl(guid));
      const presignedUrl: string = response.data;
      console.info('Retrieved presigned url: ', presignedUrl);

      await uploadFile(presignedUrl, newsFile.value!); // Await the file upload

      // Reset fields and close modal only after successful submission
      newsTitle.value = '';
      newsFile.value = null;
      isOpen.value = false;
    } catch (error: any) {
      console.error('Error during news creation:', error);
      submitError.value = true; // Indicate submission failure
    } finally {
      isLoading.value = false; // Always set loading to false when done
    }
  }
}

async function uploadFile(url: string, file: File) {
  const axiosInstance = axios.create();
  console.info('Uploading file to bucket...', file);
  try {
    const response = await axiosInstance.put(url, file, { headers: { 'Content-Type': file.type } });
    console.info('Successfully uploaded to bucket ', response);
  } catch (error) {
    console.error('Error uploading file:', error);
    throw error; // Re-throw the error so createNews can catch it
  }
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

                <button type="button" class="cursor-pointer absolute top-3 right-3 text-gray-400 hover:text-gray-500"
                  @click="closeModal" :disabled="isLoading">
                  <span class="sr-only">Close</span>
                  <svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor" aria-hidden="true">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                  </svg>
                </button>

                <DialogTitle as="h3" class="text-lg font-medium leading-6 pb-4">
                  Ny Nyhed
                </DialogTitle>

                <form @submit.prevent="createNews">
                  <div class="pb-4">
                    <label for="title_input">Titel</label>
                    <br>
                    <input
                      class="w-full bg-gray-800 border-0 rounded-xl p-2"
                      id="title_input"
                      name="title_input"
                      placeholder="Titel"
                      type="text"
                      v-model="newsTitle"
                      @keypress="handleNewsTitleChange"
                      :disabled="isLoading">
                  </div>

                  <div class="pb-4">
                    <label for="file_input">Billede</label>
                    <br>
                    <input
                      class="w-full bg-gray-800 border-0 rounded-xl p-2 cursor-pointer hover:bg-gray-600 focus:bg-gray-300 focus:text-gray-900"
                      id="file_input"
                      name="file_input"
                      type="file"
                      accept="image/*"
                      @change="handleFileUpload"
                      :disabled="isLoading">
                  </div>

                  <div v-show="fileUploadError" class="pb-4 text-red-400">
                    <span>⚠️ Kan ikke uploade en fil med filstørrelse på 0 bytes!</span>
                  </div>

                  <div v-show="submitError" class="pb-4 text-red-400">
                    <span>⚠️ Kan ikke oprette nyhed uden både titel og billede!</span>
                  </div>

                  <div class="mt-4">
                    <button type="submit"
                      class="inline-flex justify-center rounded-md border border-transparent bg-gray-300 px-4 py-2 text-md font-medium hover:bg-gray-400 focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 text-gray-900 focus-visible:ring-offset-2"
                      :disabled="isLoading">
                      <span v-if="isLoading" class="flex items-center">
                        <svg class="animate-spin -ml-1 mr-3 h-5 w-5 text-gray-900" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                          <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                          <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                        </svg>
                        Opretter Nyhed...
                      </span>
                      <span v-else>
                        Opret Nyhed
                      </span>
                    </button>
                  </div>
                </form>
              </DialogPanel>
            </TransitionChild>
          </div>
        </div>
      </Dialog>
    </TransitionRoot>
  </div>
</template>

<style scoped>
/* You might want to add some styling for the spinner if it's not handled by Tailwind */
/* For example, if you want to explicitly define the animation */
@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}
.animate-spin {
  animation: spin 1s linear infinite;
}
</style>
