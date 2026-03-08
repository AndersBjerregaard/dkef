<script setup lang="ts">
import { computed, onMounted, ref, type Ref } from 'vue'
import EventComponent from './EventComponent.vue'
import NewsComponent from './NewsComponent.vue'
import GeneralAssemblyComponent from './GeneralAssemblyComponent.vue'
import { TransitionRoot, TransitionChild, Dialog, DialogPanel, DialogTitle } from '@headlessui/vue'
import { v4 as uuidv4 } from 'uuid'
import apiservice from '@/services/apiservice'
import urlservice from '@/services/urlservice'
import type { AxiosResponse } from 'axios'
import axios from 'axios'
import { type EventDto, type PublishedEvent } from '@/types/events'
import type { NewsDto, PublishedNews } from '@/types/news'
import type { GeneralAssemblyDto, PublishedGeneralAssembly } from '@/types/generalAssembly'
import { useFeedStore } from '@/stores/feedStore'
import { useAuthStore } from '@/stores/authStore'

type FilterType = 'all' | 'events' | 'news' | 'general-assemblies'
type CreateType = 'event' | 'news' | 'general-assembly'

const feedStore = useFeedStore()
const authStore = useAuthStore()

const isFetching = ref(true)
const activeFilter = ref<FilterType>('all')

const isOpen: Ref<boolean> = ref(false)
const isLoading: Ref<boolean> = ref(false)

// Shared fields
const createType: Ref<CreateType> = ref('event')
const itemTitle: Ref<string> = ref('')
const itemFile: Ref<File | null> = ref(null)
const itemDescription: Ref<string> = ref('')
const itemSection: Ref<string> = ref('')

// Event + General Assembly fields
const itemAddress: Ref<string> = ref('')
const itemDate: Ref<string> = ref('')

// News-specific fields
const itemAuthor: Ref<string> = ref('')

const fileUploadError: Ref<boolean> = ref(false)
const submitError: Ref<boolean> = ref(false)

// Derive typed buckets from the flat feed, preserving CreatedAt sort order
const allEvents = computed(() =>
  feedStore.items
    .filter((item) => item.kind === 'event')
    .map((item) => ({
      id: item.id,
      title: item.title,
      section: item.section,
      address: item.address ?? '',
      dateTime: item.dateTime ?? '',
      description: item.description,
      thumbnailUrl: item.thumbnailUrl,
      createdAt: item.createdAt,
    })) satisfies PublishedEvent[],
)

const allNews = computed(() =>
  feedStore.items
    .filter((item) => item.kind === 'news')
    .map((item) => ({
      id: item.id,
      title: item.title,
      section: item.section,
      author: item.author ?? '',
      description: item.description,
      thumbnailUrl: item.thumbnailUrl,
      publishedAt: item.publishedAt ?? '',
      createdAt: item.createdAt,
    })) satisfies PublishedNews[],
)

const allAssemblies = computed(() =>
  feedStore.items
    .filter((item) => item.kind === 'general-assembly')
    .map((item) => ({
      id: item.id,
      title: item.title,
      section: item.section,
      address: item.address ?? '',
      dateTime: item.dateTime ?? '',
      description: item.description,
      thumbnailUrl: item.thumbnailUrl,
      createdAt: item.createdAt,
    })) satisfies PublishedGeneralAssembly[],
)

// Computed: items to display based on active filter
const displayedItems = computed(() => {
  switch (activeFilter.value) {
    case 'events':
      return { events: allEvents.value, news: [], assemblies: [] }
    case 'news':
      return { events: [], news: allNews.value, assemblies: [] }
    case 'general-assemblies':
      return { events: [], news: [], assemblies: allAssemblies.value }
    default:
      return { events: allEvents.value, news: allNews.value, assemblies: allAssemblies.value }
  }
})

const isAnyFetching = computed(() => isFetching.value || feedStore.isFetching)

async function fetchAll() {
  isFetching.value = true
  await feedStore.fetchFeed(9)
  isFetching.value = false
}

onMounted(async () => {
  await fetchAll()
})

function setFilter(filter: FilterType) {
  activeFilter.value = filter
}

function closeModal() {
  resetFields()
  isOpen.value = false
}

function openModal() {
  resetFields()
  isOpen.value = true
  submitError.value = false
  fileUploadError.value = false
}

function handleFieldChange() {
  submitError.value = false
}

function handleFileUpload(event: Event) {
  submitError.value = false
  fileUploadError.value = false
  const input = event.target as HTMLInputElement
  if (input.files && input.files.length > 0) {
    itemFile.value = input.files[0]
    if (itemFile.value.size === 0) {
      fileUploadError.value = true
    }
  } else {
    itemFile.value = null
  }
}

function resetFields() {
  createType.value = 'event'
  itemTitle.value = ''
  itemDescription.value = ''
  itemFile.value = null
  itemSection.value = ''
  itemAddress.value = ''
  itemDate.value = ''
  itemAuthor.value = ''
}

function validateFields(): boolean {
  if (itemTitle.value === '') return false
  if (itemDescription.value === '') return false

  if (createType.value === 'event' || createType.value === 'general-assembly') {
    if (itemFile.value === null) return false
    if (itemSection.value === '') return false
    if (itemAddress.value === '') return false
    if (itemDate.value === '') return false
  }

  // News: file, section and author are all optional
  return true
}

async function uploadFile(url: string, file: File) {
  const axiosInstance = axios.create()
  try {
    await axiosInstance.put(url, file, { headers: { 'Content-Type': file.type } })
  } catch (error: unknown) {
    console.error('Error uploading file:', error)
    throw error
  }
}

async function createItem() {
  submitError.value = false
  if (!validateFields()) {
    submitError.value = true
    return
  }

  isLoading.value = true
  try {
    if (createType.value === 'event') {
      await createEvent()
    } else if (createType.value === 'news') {
      await createNews()
    } else {
      await createGeneralAssembly()
    }
    resetFields()
    isOpen.value = false
    await fetchAll()
  } catch (err: unknown) {
    const axiosError = err as { response?: { data?: { message?: string } }; message?: string }
    const message =
      axiosError.response?.data?.message || axiosError.message || 'Fejl ved oprettelse. Prøv igen.'
    console.error(message)
    submitError.value = true
  } finally {
    isLoading.value = false
  }
}

async function createEvent() {
  const guid: string = uuidv4()
  const presignedUrlResponse: AxiosResponse<string> = await apiservice.get<string>(
    urlservice.getEventPresignedUrl(guid),
  )
  await uploadFile(presignedUrlResponse.data, itemFile.value!)

  const newEvent: EventDto = {
    title: itemTitle.value,
    section: itemSection.value,
    address: itemAddress.value,
    dateTime: itemDate.value,
    description: itemDescription.value,
    thumbnailId: guid,
  }
  await apiservice.post<PublishedEvent>(urlservice.postEvent(), newEvent)
}

async function createNews() {
  let thumbnailId = ''
  if (itemFile.value !== null) {
    thumbnailId = uuidv4()
    const presignedUrlResponse: AxiosResponse<string> = await apiservice.get<string>(
      urlservice.getNewsPresignedUrl(thumbnailId),
    )
    await uploadFile(presignedUrlResponse.data, itemFile.value)
  }

  const newNews: NewsDto = {
    title: itemTitle.value,
    section: itemSection.value,
    author: itemAuthor.value,
    description: itemDescription.value,
    thumbnailId,
  }
  await apiservice.post<PublishedNews>(urlservice.postNews(), newNews)
}

async function createGeneralAssembly() {
  const guid: string = uuidv4()
  const presignedUrlResponse: AxiosResponse<string> = await apiservice.get<string>(
    urlservice.getGeneralAssemblyPresignedUrl(guid),
  )
  await uploadFile(presignedUrlResponse.data, itemFile.value!)

  const newAssembly: GeneralAssemblyDto = {
    title: itemTitle.value,
    section: itemSection.value,
    address: itemAddress.value,
    dateTime: itemDate.value,
    description: itemDescription.value,
    thumbnailId: guid,
  }
  await apiservice.post<PublishedGeneralAssembly>(urlservice.postGeneralAssembly(), newAssembly)
}

const modalTitle = computed(() => {
  switch (createType.value) {
    case 'news':
      return 'Ny Nyhed'
    case 'general-assembly':
      return 'Ny Generalforsamling'
    default:
      return 'Nyt Arrangement'
  }
})

const submitLabel = computed(() => {
  if (isLoading.value) {
    switch (createType.value) {
      case 'news':
        return 'Opretter Nyhed...'
      case 'general-assembly':
        return 'Opretter Generalforsamling...'
      default:
        return 'Opretter Arrangement...'
    }
  }
  switch (createType.value) {
    case 'news':
      return 'Opret Nyhed'
    case 'general-assembly':
      return 'Opret Generalforsamling'
    default:
      return 'Opret Arrangement'
  }
})
</script>

<template>
  <div class="py-24">
    <div class="flex justify-center items-center">
      <h1 class="text-4xl">Arrangementer og Nyheder</h1>
    </div>

    <!-- Filter buttons -->
    <div class="flex justify-center items-center py-12 gap-x-8 flex-wrap gap-y-4">
      <button
        class="flex justify-center rounded-lg h-10 sm:h-12 py-2 w-24 sm:w-32 cursor-pointer sm:text-lg transition-colors font-medium"
        :class="
          activeFilter === 'all'
            ? 'bg-amber-600 text-navy-950'
            : 'bg-navy-800 text-slate-200 hover:bg-navy-700 hover:text-amber-400'
        "
        @click="setFilter('all')"
      >
        Alle
      </button>
      <button
        class="flex justify-center rounded-lg h-10 sm:h-12 py-2 w-24 sm:w-32 cursor-pointer sm:text-lg transition-colors font-medium"
        :class="
          activeFilter === 'news'
            ? 'bg-amber-600 text-navy-950'
            : 'bg-navy-800 text-slate-200 hover:bg-navy-700 hover:text-amber-400'
        "
        @click="setFilter('news')"
      >
        Nyheder
      </button>
      <button
        class="flex justify-center rounded-lg h-10 sm:h-12 py-2 w-24 sm:w-36 cursor-pointer sm:text-lg transition-colors font-medium"
        :class="
          activeFilter === 'events'
            ? 'bg-amber-600 text-navy-950'
            : 'bg-navy-800 text-slate-200 hover:bg-navy-700 hover:text-amber-400'
        "
        @click="setFilter('events')"
      >
        Arrangementer
      </button>
      <button
        class="flex justify-center rounded-lg h-10 sm:h-12 py-2 w-24 sm:w-48 cursor-pointer sm:text-lg transition-colors font-medium"
        :class="
          activeFilter === 'general-assemblies'
            ? 'bg-amber-600 text-navy-950'
            : 'bg-navy-800 text-slate-200 hover:bg-navy-700 hover:text-amber-400'
        "
        @click="setFilter('general-assemblies')"
      >
        Generalforsamlinger
      </button>
    </div>

    <div class="flex justify-center items-center">
      <h2 class="text-2xl pb-8">Seneste arrangementer og nyheder:</h2>
    </div>

    <!-- Loading spinner -->
    <div v-if="isAnyFetching" class="flex justify-center items-center min-h-[200px]">
      <svg
        class="animate-spin h-10 w-10 text-amber-500"
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
      <span class="ml-3 text-lg">Henter...</span>
    </div>

    <!-- Items grid -->
    <div v-else class="flex flex-wrap justify-center items-start gap-6 px-4">
      <EventComponent
        v-for="item in displayedItems.events"
        :key="item.id"
        :published-event="item"
      />
      <NewsComponent v-for="item in displayedItems.news" :key="item.id" :published-news="item" />
      <GeneralAssemblyComponent
        v-for="item in displayedItems.assemblies"
        :key="item.id"
        :published-general-assembly="item"
      />
    </div>

    <!-- Admin create button -->
    <div class="flex justify-center items-center py-12 gap-x-8" v-if="authStore.isAdmin">
      <button
        class="flex justify-center rounded-lg bg-navy-800 text-slate-200 h-10 sm:h-12 py-2 w-24 sm:w-48 cursor-pointer hover:bg-navy-700 hover:text-amber-400 sm:text-lg transition-colors"
        @click="openModal"
      >
        Opret ny...
      </button>
    </div>

    <!-- Creation modal -->
    <TransitionRoot appear :show="isOpen" as="template">
      <Dialog as="div" @close="closeModal" class="relative z-10">
        <TransitionChild
          as="template"
          enter="duration-300 ease-out"
          enter-from="opacity-0"
          enter-to="opacity-100"
          leave="duration-200 ease-in"
          leave-from="opacity-100"
          leave-to="opacity-0"
        >
          <div class="fixed inset-0 bg-black/50"></div>
        </TransitionChild>

        <div class="fixed inset-0 overflow-y-auto">
          <div class="flex min-h-full items-center justify-center p-8 text-center">
            <TransitionChild
              as="template"
              enter="duration-300 ease-out"
              enter-from="opacity-0 scale-95"
              enter-to="opacity-100 scale-100"
              leave="duration-200 ease-in"
              leave-from="opacity-100 scale-100"
              leave-to="opacity-0 scale-95"
            >
              <DialogPanel
                class="w-full transform overflow-hidden rounded-2xl bg-navy-800 border border-navy-700 p-6 text-left align-middle shadow-xl transition-all origin-center translate-z-0"
              >
                <button
                  type="button"
                  class="cursor-pointer absolute top-3 right-3 text-slate-400 hover:text-amber-400 transition-colors"
                  @click="closeModal"
                  :disabled="isLoading"
                >
                  <span class="sr-only">Luk</span>
                  <svg
                    class="h-6 w-6"
                    fill="none"
                    viewBox="0 0 24 24"
                    stroke="currentColor"
                    aria-hidden="true"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M6 18L18 6M6 6l12 12"
                    />
                  </svg>
                </button>

                <DialogTitle as="h3" class="text-lg font-medium leading-6 pb-4">
                  {{ modalTitle }}
                </DialogTitle>

                <form @submit.prevent="createItem">
                  <!-- Type selector -->
                  <div class="pb-4">
                    <label for="type_select">Type</label>
                    <br />
                    <select
                      id="type_select"
                      class="w-full bg-navy-900 border border-navy-700 rounded-xl p-2 cursor-pointer focus:outline-none focus:ring-2 focus:ring-amber-500"
                      v-model="createType"
                      :disabled="isLoading"
                      @change="handleFieldChange"
                    >
                      <option value="event">Arrangement</option>
                      <option value="news">Nyhed</option>
                      <option value="general-assembly">Generalforsamling</option>
                    </select>
                  </div>

                  <!-- Shared: Title -->
                  <div class="pb-4">
                    <label for="title_input">Titel</label>
                    <br />
                    <input
                      class="w-full bg-navy-900 border border-navy-700 rounded-xl p-2 focus:outline-none focus:ring-2 focus:ring-amber-500"
                      id="title_input"
                      placeholder="Titel"
                      type="text"
                      v-model="itemTitle"
                      @keypress="handleFieldChange"
                      :disabled="isLoading"
                    />
                  </div>

                  <!-- Event + General Assembly fields -->
                  <template v-if="createType === 'event' || createType === 'general-assembly'">
                    <div class="flex justify-between gap-4 pb-4">
                      <div class="flex-1">
                        <label for="section_input">Sektion</label>
                        <br />
                        <input
                          class="w-full bg-navy-900 border border-navy-700 rounded-xl p-2 focus:outline-none focus:ring-2 focus:ring-amber-500"
                          id="section_input"
                          placeholder="Sektion"
                          type="text"
                          v-model="itemSection"
                          @keypress="handleFieldChange"
                          :disabled="isLoading"
                        />
                      </div>
                      <div class="flex-1">
                        <label for="address_input">Adresse</label>
                        <br />
                        <input
                          class="w-full bg-navy-900 border border-navy-700 rounded-xl p-2 focus:outline-none focus:ring-2 focus:ring-amber-500"
                          id="address_input"
                          placeholder="Adresse"
                          type="text"
                          v-model="itemAddress"
                          @keypress="handleFieldChange"
                          :disabled="isLoading"
                        />
                      </div>
                      <div class="flex-1">
                        <label for="date_input">Dato</label>
                        <br />
                        <input
                          class="w-full bg-navy-900 border border-navy-700 rounded-xl p-2 cursor-pointer focus:outline-none focus:ring-2 focus:ring-amber-500"
                          id="date_input"
                          type="datetime-local"
                          v-model="itemDate"
                          @click="handleFieldChange"
                          :disabled="isLoading"
                        />
                      </div>
                    </div>
                  </template>

                  <!-- News-specific optional fields -->
                  <template v-if="createType === 'news'">
                    <div class="flex gap-4 pb-4">
                      <div class="flex-1">
                        <label for="section_input_news">Sektion (valgfri)</label>
                        <br />
                        <input
                          class="w-full bg-navy-900 border border-navy-700 rounded-xl p-2 focus:outline-none focus:ring-2 focus:ring-amber-500"
                          id="section_input_news"
                          placeholder="Sektion"
                          type="text"
                          v-model="itemSection"
                          @keypress="handleFieldChange"
                          :disabled="isLoading"
                        />
                      </div>
                      <div class="flex-1">
                        <label for="author_input">Forfatter (valgfri)</label>
                        <br />
                        <input
                          class="w-full bg-navy-900 border border-navy-700 rounded-xl p-2 focus:outline-none focus:ring-2 focus:ring-amber-500"
                          id="author_input"
                          placeholder="Forfatter"
                          type="text"
                          v-model="itemAuthor"
                          @keypress="handleFieldChange"
                          :disabled="isLoading"
                        />
                      </div>
                    </div>
                  </template>

                  <!-- Shared: Description -->
                  <div class="pb-4">
                    <label for="description_input">Beskrivelse</label>
                    <br />
                    <textarea
                      class="w-full bg-navy-900 border border-navy-700 rounded-xl p-2 h-96 focus:outline-none focus:ring-2 focus:ring-amber-500"
                      id="description_input"
                      placeholder="Beskrivelse"
                      v-model="itemDescription"
                      @keypress="handleFieldChange"
                      :disabled="isLoading"
                    ></textarea>
                  </div>

                  <!-- File upload: required for event/general-assembly, optional for news -->
                  <div class="pb-4">
                    <label for="file_input">
                      Billede{{ createType === 'news' ? ' (valgfri)' : '' }}
                    </label>
                    <br />
                    <input
                      class="w-full bg-navy-900 border border-navy-700 rounded-xl p-2 cursor-pointer hover:bg-navy-800 focus:outline-none focus:ring-2 focus:ring-amber-500"
                      id="file_input"
                      type="file"
                      accept="image/*"
                      @change="handleFileUpload"
                      :disabled="isLoading"
                    />
                  </div>

                  <div v-show="fileUploadError" class="pb-4 text-red-400">
                    <span>Kan ikke uploade en fil med filstørrelse på 0 bytes!</span>
                  </div>

                  <div v-show="submitError" class="pb-4 text-red-400">
                    <span>Venligst udfyld alle påkrævede felter</span>
                  </div>

                  <div class="mt-4">
                    <button
                      type="submit"
                      class="inline-flex justify-center rounded-md border border-transparent bg-amber-600 text-navy-950 px-4 py-2 text-md font-semibold hover:bg-amber-500 focus:outline-none focus-visible:ring-2 focus-visible:ring-amber-500 focus-visible:ring-offset-2 shadow-lg shadow-amber-600/20 transition-colors"
                      :disabled="isLoading"
                    >
                      <span v-if="isLoading" class="flex items-center">
                        <svg
                          class="animate-spin -ml-1 mr-3 h-5 w-5 text-navy-950"
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
                        {{ submitLabel }}
                      </span>
                      <span v-else>{{ submitLabel }}</span>
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

<style scoped></style>
