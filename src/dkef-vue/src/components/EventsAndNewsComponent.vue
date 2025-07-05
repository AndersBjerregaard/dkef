<script setup lang="ts">
import { onMounted, ref, type Ref } from 'vue';
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
import { type EventsCollection, type EventDto, type PublishedEvent } from '@/types/events';

// Example items
const publishedEvents: Ref<PublishedEvent[]> = ref([]);

const isOpen: Ref<boolean> = ref(false);
const isLoading: Ref<boolean> = ref(false);

const eventTitle: Ref<string> = ref('');
const eventFile: Ref<File | null> = ref(null);
const eventDescription: Ref<string> = ref('');
const eventSection: Ref<string> = ref('');
const eventAddress: Ref<string> = ref('');
const eventDate: Ref<string> = ref('');

const fileUploadError: Ref<boolean> = ref(false);
const submitError: Ref<boolean> = ref(false);

async function fetchLatestPublishedEvents() {
  try {
    const response = await apiservice.get<EventsCollection>(urlservice.getEvents(), {
      params: {
        take: 3,
        orderBy: 'createdAt',
        order: "desc"
      }
    });
    publishedEvents.value = response.data.collection;
  } catch (error) {
    console.error('Error retrieving published events: ', error);
  }
}

onMounted(fetchLatestPublishedEvents);

function closeModal() {
  resetFields();

  isOpen.value = false;
}

function openModal() {
  resetFields();

  isOpen.value = true;
  submitError.value = false;
  fileUploadError.value = false;
}

function handleEventTitleChange(event: Event) {
  submitError.value = false;
}

function handleFileUpload(event: Event) {
  submitError.value = false;
  fileUploadError.value = false;
  const input = event.target as HTMLInputElement;
  if (input.files && input.files.length > 0) {
    eventFile.value = input.files[0];
    if (eventFile.value.size === 0) {
      fileUploadError.value = true;
    }
  } else {
    eventFile.value = null; // Clear if no file is selected (e.g. user cancels)
  }
}

function resetFields() {
  eventTitle.value = '';
  eventDescription.value = '';
  eventFile.value = null;
  eventSection.value = '';
  eventAddress.value = '';
  eventDate.value = '';
}

function validateFields(): boolean {
  if (eventTitle.value === '') return false;
  if (eventFile.value === null) return false;
  if (eventDescription.value === '') return false;
  if (eventSection.value === '') return false;
  if (eventAddress.value === '') return false;
  if (eventDate.value === '') return false;
  return true;
}

async function createEvent() {
  submitError.value = false;
  if (!validateFields()) {
    submitError.value = true;
    return;
  }

  if (!submitError.value) {
    isLoading.value = true; // Set loading to true
    try {
      const guid: string = uuidv4(); // Generated guid to identify the uploaded file
      const presignedUrlResponse: AxiosResponse<string> = await apiservice.get<string>(urlservice.getEventPresignedUrl(guid));
      const presignedUrl: string = presignedUrlResponse.data;

      await uploadFile(presignedUrl, eventFile.value!);

      const newEvent: EventDto = {
        title: eventTitle.value,
        section: eventSection.value,
        address: eventAddress.value,
        dateTime: eventDate.value,
        description: eventDescription.value,
        thumbnailId: guid
      }

      const eventPostResponse: AxiosResponse<any> = await apiservice.post<any>(urlservice.postEvent(), newEvent);

      // Reset fields and close modal only after successful submission
      resetFields();
      isOpen.value = false;
    } catch (error: any) {
      console.error('Error during event creation:', error);
      submitError.value = true; // Indicate submission failure
    } finally {
      isLoading.value = false; // Always set loading to false when done
    }
  }
}

async function uploadFile(url: string, file: File) {
  const axiosInstance = axios.create();
  try {
    await axiosInstance.put(url, file, { headers: { 'Content-Type': file.type } });
  } catch (error) {
    console.error('Error uploading file:', error);
    throw error; // Re-throw the error so createEvent can catch it
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
    <div class="flex justify-center items-center">
      <h2 class="text-2xl pb-8">Seneste arrangementer og nyheder:</h2>
    </div>
    <div class="flex flex-wrap justify-center items-stretch gap-8">
      <EventComponent v-for="item in publishedEvents" :key="item.id" :published-event="item"/>
    </div>
    <div class="flex justify-center items-center py-12 gap-x-8">
      <button class="flex justify-center rounded bg-gray-600 h-10 sm:h-12 py-2
        w-24 sm:w-48 cursor-pointer hover:bg-gray-800 sm:text-lg" @click="openModal">
        Nyt Arrangement
      </button>
    </div>
    <TransitionRoot appear :show="isOpen" as="template">
      <Dialog as="div" @close="closeModal" class="relative z-10">
        <TransitionChild as="template" enter="duration-300 ease-out" enter-from="opacity-0" enter-to="opacity-100"
          leave="duration-200 ease-in" leave-from="opacity-100" leave-to="opacity-0">
          <div class="fixed inset-0 bg-black/25"></div>
        </TransitionChild>

        <div class="fixed inset-0 overflow-y-auto">
          <div class="flex min-h-full items-center justify-center p-8 text-center">
            <TransitionChild as="template" enter="duration-300 ease-out" enter-from="opacity-0 scale-95"
              enter-to="opacity-100 scale-100" leave="duration-200 ease-in" leave-from="opacity-100 scale-100"
              leave-to="opacity-0 scale-95">
              <DialogPanel
                class="w-full transform overflow-hidden rounded-2xl bg-gray-700 p-6 text-left align-middle shadow-xl transition-all **origin-center** translate-z-0 border">

                <button type="button" class="cursor-pointer absolute top-3 right-3 text-gray-400 hover:text-gray-500"
                  @click="closeModal" :disabled="isLoading">
                  <span class="sr-only">Close</span>
                  <svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor" aria-hidden="true">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                  </svg>
                </button>

                <DialogTitle as="h3" class="text-lg font-medium leading-6 pb-4">
                  Nyt Arrangement
                </DialogTitle>

                <form @submit.prevent="createEvent">
                  <div class="flex justify-between">
                    <div class="pb-4">
                      <label for="title_input">Titel</label>
                      <br>
                      <input class="w-full bg-gray-800 border-0 rounded-xl p-2" id="title_input" name="title_input"
                        placeholder="Titel" type="text" v-model="eventTitle" @keypress="handleEventTitleChange"
                        :disabled="isLoading">
                    </div>

                    <div class="pb-4">
                      <label for="section_input">Sektion</label>
                      <br>
                      <input class="w-full bg-gray-800 border-0 rounded-xl p-2" id="section_input" name="section_input"
                        placeholder="Sektion" type="text" v-model="eventSection" @keypress="handleEventTitleChange"
                        :disabled="isLoading">
                    </div>

                    <div class="pb-4">
                      <label for="address_input">Addresse</label>
                      <br>
                      <input class="w-full bg-gray-800 border-0 rounded-xl p-2" id="address_input" name="address_input"
                        placeholder="Addresse" type="text" v-model="eventAddress" @keypress="handleEventTitleChange"
                        :disabled="isLoading">
                    </div>

                    <div class="pb-4">
                      <label for="date_input">Dato</label>
                      <br>
                      <input class="w-full bg-gray-800 border-0 rounded-xl p-2 cursor-pointer" id="date_input"
                        name="date_input" type="datetime-local" v-model="eventDate" @click="handleEventTitleChange"
                        :disabled="isLoading">
                    </div>
                  </div>

                  <div class="pb-4">
                    <label for="description_input">Beskrivelse</label>
                    <br>
                    <textarea class="w-full bg-gray-800 border-0 rounded-xl p-2 h-96" id="description_input"
                      name="description_input" placeholder="Beskrivelse" type="text" v-model="eventDescription"
                      @keypress="handleEventTitleChange" :disabled="isLoading">
                    </textarea>
                  </div>

                  <div class="pb-4">
                    <label for="file_input">Billede</label>
                    <br>
                    <input
                      class="w-full bg-gray-800 border-0 rounded-xl p-2 cursor-pointer hover:bg-gray-600 focus:bg-gray-300 focus:text-gray-900"
                      id="file_input" name="file_input" type="file" accept="image/*" @change="handleFileUpload"
                      :disabled="isLoading">
                  </div>

                  <div v-show="fileUploadError" class="pb-4 text-red-400">
                    <span>⚠️ Kan ikke uploade en fil med filstørrelse på 0 bytes!</span>
                  </div>

                  <div v-show="submitError" class="pb-4 text-red-400">
                    <span>⚠️ Venligst udfyld alle felter</span>
                  </div>

                  <div class="mt-4">
                    <button type="submit"
                      class="inline-flex justify-center rounded-md border border-transparent bg-gray-300 px-4 py-2 text-md font-medium hover:bg-gray-400 focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 text-gray-900 focus-visible:ring-offset-2"
                      :disabled="isLoading">
                      <span v-if="isLoading" class="flex items-center">
                        <svg class="animate-spin -ml-1 mr-3 h-5 w-5 text-gray-900" xmlns="http://www.w3.org/2000/svg"
                          fill="none" viewBox="0 0 24 24">
                          <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4">
                          </circle>
                          <path class="opacity-75" fill="currentColor"
                            d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z">
                          </path>
                        </svg>
                        Opretter Arrangement...
                      </span>
                      <span v-else>
                        Opret Arrangement
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
