<script setup lang="ts">
import { computed, onMounted, ref, type Ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import EventComponent from './EventComponent.vue'
import NewsComponent from './NewsComponent.vue'
import GeneralAssemblyComponent from './GeneralAssemblyComponent.vue'
import BaseModal from '@/components/BaseModal.vue'
import { v4 as uuidv4 } from 'uuid'
import apiservice from '@/services/apiservice'
import urlservice from '@/services/urlservice'
import type { AxiosResponse } from 'axios'
import axios from 'axios'
import { type EventDto, type PublishedEvent, type EventsCollection } from '@/types/events'
import type { NewsDto, PublishedNews, NewsCollection } from '@/types/news'
import type {
  GeneralAssemblyDto,
  PublishedGeneralAssembly,
  GeneralAssemblyCollection,
} from '@/types/generalAssembly'
import type { FeedItem, FeedResponse } from '@/types/feed'
import { useFeedStore } from '@/stores/feedStore'
import { useAuthStore } from '@/stores/authStore'

type FilterType = 'all' | 'events' | 'news' | 'general-assemblies'
type CreateType = 'event' | 'news' | 'general-assembly'

const feedStore = useFeedStore()
const authStore = useAuthStore()
const router = useRouter()
const route = useRoute()

const isFetching = ref(true)
const isFilterFetching = ref(false)
const activeFilter = ref<FilterType>('all')
const pageSize = 9

// Pagination state per filter type
const currentPageAll = ref(1)
const currentPageEvents = ref(1)
const currentPageNews = ref(1)
const currentPageAssemblies = ref(1)

// Total counts per filter type
const totalEvents = ref(0)
const totalNews = ref(0)
const totalAssemblies = ref(0)

// Per-type result caches for specific filter views
const filteredEvents = ref<PublishedEvent[]>([])
const filteredNews = ref<PublishedNews[]>([])
const filteredAssemblies = ref<PublishedGeneralAssembly[]>([])

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

// Flat list in server sort order
const displayedItems = computed<FeedItem[]>(() => {
  switch (activeFilter.value) {
    case 'events':
      return filteredEvents.value.map((e) => ({
        id: e.id,
        kind: 'event' as const,
        title: e.title,
        section: e.section,
        description: e.description,
        thumbnailUrl: e.thumbnailUrl,
        createdAt: e.createdAt,
        address: e.address,
        dateTime: e.dateTime,
      }))
    case 'news':
      return filteredNews.value.map((n) => ({
        id: n.id,
        kind: 'news' as const,
        title: n.title,
        section: n.section,
        description: n.description,
        thumbnailUrl: n.thumbnailUrl,
        createdAt: n.createdAt,
        author: n.author,
        publishedAt: n.publishedAt,
      }))
    case 'general-assemblies':
      return filteredAssemblies.value.map((g) => ({
        id: g.id,
        kind: 'general-assembly' as const,
        title: g.title,
        section: g.section,
        description: g.description,
        thumbnailUrl: g.thumbnailUrl,
        createdAt: g.createdAt,
        address: g.address,
        dateTime: g.dateTime,
      }))
    default:
      return feedStore.items
  }
})

const isAnyFetching = computed(
  () => isFetching.value || feedStore.isFetching || isFilterFetching.value,
)

const getCurrentPage = computed(() => {
  switch (activeFilter.value) {
    case 'events':
      return currentPageEvents.value
    case 'news':
      return currentPageNews.value
    case 'general-assemblies':
      return currentPageAssemblies.value
    default:
      return currentPageAll.value
  }
})

const getTotalItems = computed(() => {
  switch (activeFilter.value) {
    case 'events':
      return totalEvents.value
    case 'news':
      return totalNews.value
    case 'general-assemblies':
      return totalAssemblies.value
    default:
      return feedStore.total
  }
})

const totalPages = computed(() => Math.ceil(getTotalItems.value / pageSize))
const hasNextPage = computed(() => getCurrentPage.value < totalPages.value)
const hasPreviousPage = computed(() => getCurrentPage.value > 1)

// Generate page numbers to display with ellipsis
const pageNumbers = computed(() => {
  const pages: (number | string)[] = []
  const maxPagesToShow = 3 // Pages to show on either side of current
  const total = totalPages.value

  // Always show first page
  pages.push(1)

  // Add ellipsis and pages before current
  if (getCurrentPage.value > maxPagesToShow + 2) {
    pages.push('...')
  }
  for (
    let i = Math.max(2, getCurrentPage.value - maxPagesToShow);
    i < Math.min(getCurrentPage.value, total);
    i++
  ) {
    pages.push(i)
  }

  // Add current page
  if (getCurrentPage.value !== 1 && getCurrentPage.value !== total) {
    pages.push(getCurrentPage.value)
  }

  // Add pages after current
  for (
    let i = Math.max(getCurrentPage.value + 1, getCurrentPage.value + 1);
    i <= Math.min(getCurrentPage.value + maxPagesToShow, total - 1);
    i++
  ) {
    pages.push(i)
  }

  // Add ellipsis and last page
  if (getCurrentPage.value < total - maxPagesToShow - 1) {
    pages.push('...')
  }
  if (total > 1) {
    pages.push(total)
  }

  // Remove duplicates
  return [...new Set(pages)]
})

async function fetchFiltered(filter: FilterType, page: number = 1) {
  if (filter === 'all') {
    isFilterFetching.value = true
    try {
      const skip = (page - 1) * pageSize
      const response = await apiservice.get<FeedResponse>(urlservice.getFeed(), {
        params: { take: pageSize, skip },
        skipAuth: true,
      })
      feedStore.items = response.data.collection
      feedStore.total = response.data.total
      currentPageAll.value = page
    } catch (err: unknown) {
      console.error('Error fetching feed items:', err)
    } finally {
      isFilterFetching.value = false
    }
    return
  }

  isFilterFetching.value = true
  try {
    const skip = (page - 1) * pageSize

    if (filter === 'events') {
      const response = await apiservice.get<EventsCollection>(urlservice.getEvents(), {
        params: {
          take: pageSize,
          skip,
          orderBy: 'DateTime',
          order: 'desc',
        },
        skipAuth: true,
      })
      filteredEvents.value = response.data.collection
      totalEvents.value = response.data.total
      currentPageEvents.value = page
    } else if (filter === 'news') {
      const response = await apiservice.get<NewsCollection>(urlservice.getNews(), {
        params: {
          take: pageSize,
          skip,
          orderBy: 'PublishedAt',
          order: 'desc',
        },
        skipAuth: true,
      })
      filteredNews.value = response.data.collection
      totalNews.value = response.data.total
      currentPageNews.value = page
    } else if (filter === 'general-assemblies') {
      const response = await apiservice.get<GeneralAssemblyCollection>(
        urlservice.getGeneralAssemblies(),
        {
          params: {
            take: pageSize,
            skip,
            orderBy: 'DateTime',
            order: 'desc',
          },
          skipAuth: true,
        },
      )
      filteredAssemblies.value = response.data.collection
      totalAssemblies.value = response.data.total
      currentPageAssemblies.value = page
    }
  } catch (err: unknown) {
    console.error('Error fetching filtered items:', err)
  } finally {
    isFilterFetching.value = false
  }
}

function goToPage(page: number) {
  if (page < 1 || page > totalPages.value) return

  const newPage = Math.max(1, Math.min(page, totalPages.value))
  router.push({ query: { filter: activeFilter.value, page: newPage } })
  fetchFiltered(activeFilter.value, newPage)
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

function nextPage() {
  if (hasNextPage.value) {
    goToPage(getCurrentPage.value + 1)
  }
}

function previousPage() {
  if (hasPreviousPage.value) {
    goToPage(getCurrentPage.value - 1)
  }
}

onMounted(async () => {
  const filter = (route.query.filter as FilterType) || 'all'
  const page = parseInt(route.query.page as string) || 1

  activeFilter.value = filter
  isFetching.value = true

  try {
    await fetchFiltered(filter, page)
  } finally {
    isFetching.value = false
  }
})

function setFilter(filter: FilterType) {
  activeFilter.value = filter
  router.push({ query: { filter, page: 1 } })
  fetchFiltered(filter, 1)
  window.scrollTo({ top: 0, behavior: 'smooth' })
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
    if (itemSection.value === '') return false
    if (itemAddress.value === '') return false
    if (itemDate.value === '') return false
  }

  // Image is optional for all types (event, news, general-assembly)
  // Section and author are optional for news only
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
    // Refresh current page after creation
    await fetchFiltered(activeFilter.value, getCurrentPage.value)
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
  let thumbnailId: string | undefined = undefined

  if (itemFile.value !== null) {
    thumbnailId = uuidv4()
    const presignedUrlResponse: AxiosResponse<string> = await apiservice.get<string>(
      urlservice.getEventPresignedUrl(thumbnailId),
    )
    await uploadFile(presignedUrlResponse.data, itemFile.value)
  }

  const newEvent: EventDto = {
    title: itemTitle.value,
    section: itemSection.value,
    address: itemAddress.value,
    dateTime: itemDate.value,
    description: itemDescription.value,
    ...(thumbnailId && { thumbnailId }),
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
  let thumbnailId: string | undefined = undefined

  if (itemFile.value !== null) {
    thumbnailId = uuidv4()
    const presignedUrlResponse: AxiosResponse<string> = await apiservice.get<string>(
      urlservice.getGeneralAssemblyPresignedUrl(thumbnailId),
    )
    await uploadFile(presignedUrlResponse.data, itemFile.value)
  }

  const newAssembly: GeneralAssemblyDto = {
    title: itemTitle.value,
    section: itemSection.value,
    address: itemAddress.value,
    dateTime: itemDate.value,
    description: itemDescription.value,
    ...(thumbnailId && { thumbnailId }),
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
            : 'bg-theme-mute text-theme-heading hover:bg-theme-border hover:text-theme-accent'
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
            : 'bg-theme-mute text-theme-heading hover:bg-theme-border hover:text-theme-accent'
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
            : 'bg-theme-mute text-theme-heading hover:bg-theme-border hover:text-theme-accent'
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
            : 'bg-theme-mute text-theme-heading hover:bg-theme-border hover:text-theme-accent'
        "
        @click="setFilter('general-assemblies')"
      >
        Generalforsamlinger
      </button>
    </div>

    <div class="flex justify-center items-center">
      <h2 class="text-2xl pb-4">Seneste arrangementer og nyheder:</h2>
    </div>

    <!-- Page info -->
    <div class="flex justify-center items-center pb-8 text-sm">
      <span class="text-theme-text">
        Side {{ getCurrentPage }} af {{ totalPages }}
        {{
          activeFilter === 'events'
            ? `(${totalEvents} arrangementer)`
            : activeFilter === 'news'
              ? `(${totalNews} nyheder)`
              : activeFilter === 'general-assemblies'
                ? `(${totalAssemblies} generalforsamlinger)`
                : `(${feedStore.total} emner)`
        }}
      </span>
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
    <div class="flex justify-center items-center" v-else>
      <div
        class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 auto-rows-fr gap-6 px-4 max-w-5xl mx-auto w-full"
      >
        <template v-for="item in displayedItems" :key="item.id">
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
              createdAt: item.createdAt,
            }"
          />
          <NewsComponent
            v-else-if="item.kind === 'news'"
            :published-news="{
              id: item.id,
              title: item.title,
              section: item.section,
              author: item.author ?? '',
              description: item.description,
              thumbnailUrl: item.thumbnailUrl,
              publishedAt: item.publishedAt ?? '',
              createdAt: item.createdAt,
            }"
          />
          <GeneralAssemblyComponent
            v-else-if="item.kind === 'general-assembly'"
            :published-general-assembly="{
              id: item.id,
              title: item.title,
              section: item.section,
              address: item.address ?? '',
              dateTime: item.dateTime ?? '',
              description: item.description,
              thumbnailUrl: item.thumbnailUrl,
              createdAt: item.createdAt,
            }"
          />
        </template>
      </div>
    </div>
    <!-- Pagination controls -->
    <div v-if="totalPages > 1" class="flex justify-center items-center py-12 gap-2">
      <button
        :disabled="!hasPreviousPage || isAnyFetching"
        @click="previousPage"
        class="px-4 py-2 rounded-lg bg-theme-mute text-theme-heading hover:bg-theme-border hover:text-theme-accent disabled:opacity-50 disabled:cursor-not-allowed transition-colors font-medium cursor-pointer"
      >
        ← Forrige
      </button>

      <!-- Page numbers with ellipsis -->
      <div class="flex gap-1 items-center">
        <template v-for="(page, idx) in pageNumbers" :key="`page-${idx}`">
          <span v-if="page === '...'" class="text-theme-text px-2">…</span>
          <button
            v-else
            :class="{
              'bg-amber-600 text-navy-950 font-bold': page === getCurrentPage,
              'bg-theme-mute text-theme-heading hover:bg-theme-border hover:text-theme-accent':
                page !== getCurrentPage,
            }"
            @click="goToPage(page as number)"
            :disabled="isAnyFetching"
            class="min-w-10 h-10 rounded-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed font-medium cursor-pointer"
          >
            {{ page }}
          </button>
        </template>
      </div>

      <button
        :disabled="!hasNextPage || isAnyFetching"
        @click="nextPage"
        class="px-4 py-2 rounded-lg bg-theme-mute text-theme-heading hover:bg-theme-border hover:text-theme-accent disabled:opacity-50 disabled:cursor-not-allowed transition-colors font-medium cursor-pointer"
      >
        Næste →
      </button>
    </div>

    <!-- Admin create button -->
    <div class="flex justify-center items-center py-12 gap-x-8" v-if="authStore.isAdmin">
      <button
        class="flex justify-center rounded-lg bg-theme-mute text-theme-heading h-10 sm:h-12 py-2 w-24 sm:w-48 cursor-pointer hover:bg-theme-border hover:text-theme-accent sm:text-lg transition-colors"
        @click="openModal"
      >
        Opret ny...
      </button>
    </div>

    <!-- Creation modal -->
    <BaseModal :is-open="isOpen" :title="modalTitle" :is-loading="isLoading" @close="closeModal">
      <form @submit.prevent="createItem">
        <!-- Type selector -->
        <div class="pb-4">
          <label for="type_select">Type</label>
          <br />
          <select
            id="type_select"
            class="w-full bg-theme-soft border border-theme-border rounded-xl p-2 cursor-pointer focus:outline-none focus:ring-2 focus:ring-theme-accent"
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
            class="w-full bg-theme-soft border border-theme-border rounded-xl p-2 focus:outline-none focus:ring-2 focus:ring-theme-accent"
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
                class="w-full bg-theme-soft border border-theme-border rounded-xl p-2 focus:outline-none focus:ring-2 focus:ring-theme-accent"
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
                class="w-full bg-theme-soft border border-theme-border rounded-xl p-2 focus:outline-none focus:ring-2 focus:ring-theme-accent"
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
                class="w-full bg-theme-soft border border-theme-border rounded-xl p-2 cursor-pointer focus:outline-none focus:ring-2 focus:ring-theme-accent"
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
                class="w-full bg-theme-soft border border-theme-border rounded-xl p-2 focus:outline-none focus:ring-2 focus:ring-theme-accent"
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
                class="w-full bg-theme-soft border border-theme-border rounded-xl p-2 focus:outline-none focus:ring-2 focus:ring-theme-accent"
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
            class="w-full bg-theme-soft border border-theme-border rounded-xl p-2 h-96 focus:outline-none focus:ring-2 focus:ring-theme-accent"
            id="description_input"
            placeholder="Beskrivelse"
            v-model="itemDescription"
            @keypress="handleFieldChange"
            :disabled="isLoading"
          ></textarea>
        </div>

        <!-- File upload: required for event/general-assembly, optional for news -->
        <div class="pb-4">
          <label for="file_input"> Billede{{ createType === 'news' ? ' (valgfri)' : '' }} </label>
          <br />
          <input
            class="w-full bg-theme-soft border border-theme-border rounded-xl p-2 cursor-pointer hover:bg-theme-mute focus:outline-none focus:ring-2 focus:ring-theme-accent"
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
            class="cursor-pointer inline-flex justify-center rounded-md border border-transparent bg-amber-600 text-navy-950 px-4 py-2 text-md font-semibold hover:bg-amber-500 focus:outline-none focus-visible:ring-2 focus-visible:ring-theme-accent focus-visible:ring-offset-2 shadow-lg shadow-amber-600/20 transition-colors disabled:opacity-50"
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
    </BaseModal>
  </div>
</template>

<style scoped></style>
