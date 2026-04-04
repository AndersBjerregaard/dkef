<script setup lang="ts">
import { ref, watch, type Ref } from 'vue'
import { v4 as uuidv4 } from 'uuid'
import axios from 'axios'
import type { AxiosResponse } from 'axios'
import BaseModal from '@/components/BaseModal.vue'
import apiservice from '@/services/apiservice'
import urlservice from '@/services/urlservice'
import type { NewsDto, PublishedNews } from '@/types/news'
import { useNewsStore } from '@/stores/newsStore'

const props = defineProps<{
  isOpen: boolean
  news: PublishedNews
}>()

const emit = defineEmits<{
  (e: 'close'): void
}>()

const newsStore = useNewsStore()

const itemTitle: Ref<string> = ref('')
const itemSection: Ref<string> = ref('')
const itemDescription: Ref<string> = ref('')
const itemFile: Ref<File | null> = ref(null)

const isLoading: Ref<boolean> = ref(false)
const fileUploadError: Ref<boolean> = ref(false)
const submitError: Ref<string | null> = ref(null)

function populateFields() {
  itemTitle.value = props.news.title
  itemSection.value = props.news.section
  itemDescription.value = props.news.description
  itemFile.value = null
  fileUploadError.value = false
  submitError.value = null
}

watch(
  () => props.isOpen,
  (open) => {
    if (open) populateFields()
  },
  { immediate: true },
)

function handleFileUpload(event: Event) {
  fileUploadError.value = false
  submitError.value = null
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

function validateFields(): boolean {
  if (itemTitle.value.trim() === '') return false
  if (itemDescription.value.trim() === '') return false
  return true
}

async function uploadFile(url: string, file: File): Promise<void> {
  const axiosInstance = axios.create()
  await axiosInstance.put(url, file, { headers: { 'Content-Type': file.type } })
}

function extractThumbnailId(thumbnailUrl: string): string {
  if (!thumbnailUrl) return ''
  try {
    const url = new URL(thumbnailUrl)
    const parts = url.pathname.split('/')
    return parts[parts.length - 1]
  } catch {
    return uuidv4()
  }
}

async function saveNews() {
  submitError.value = null
  if (!validateFields()) {
    submitError.value = 'Venligst udfyld alle påkrævede felter'
    return
  }
  if (fileUploadError.value) return

  isLoading.value = true
  try {
    let thumbnailId = extractThumbnailId(props.news.thumbnailUrl)

    if (itemFile.value !== null) {
      const newGuid = uuidv4()
      const presignedUrlResponse: AxiosResponse<string> = await apiservice.get<string>(
        urlservice.getNewsPresignedUrl(newGuid),
      )
      await uploadFile(presignedUrlResponse.data, itemFile.value)
      thumbnailId = newGuid
    }

    const dto: NewsDto = {
      title: itemTitle.value,
      section: itemSection.value,
      description: itemDescription.value,
      thumbnailId,
    }

    await newsStore.updateNewsItem(props.news.id, dto)
    emit('close')
  } catch (err: unknown) {
    const axiosError = err as { response?: { data?: { message?: string } }; message?: string }
    submitError.value =
      axiosError.response?.data?.message || axiosError.message || 'Fejl ved opdatering. Prøv igen.'
    console.error(err)
  } finally {
    isLoading.value = false
  }
}
</script>

<template>
  <BaseModal :is-open="isOpen" title="Rediger Nyhed" :is-loading="isLoading" @close="emit('close')">
    <form @submit.prevent="saveNews">
      <!-- Title -->
      <div class="pb-4">
        <label for="edit_news_title">Titel</label>
        <br />
        <input
          id="edit_news_title"
          class="w-full bg-theme-soft border border-theme-border rounded-xl p-2 text-theme-heading placeholder-theme-muted focus:outline-none focus:ring-2 focus:ring-theme-accent focus:border-theme-accent"
          type="text"
          placeholder="Titel"
          v-model="itemTitle"
          :disabled="isLoading"
        />
      </div>

      <!-- Section (optional) -->
      <div class="flex gap-4 pb-4">
        <div class="flex-1">
          <label for="edit_news_section">Sektion (valgfri)</label>
          <br />
          <input
            id="edit_news_section"
            class="w-full bg-theme-soft border border-theme-border rounded-xl p-2 text-theme-heading placeholder-theme-muted focus:outline-none focus:ring-2 focus:ring-theme-accent focus:border-theme-accent"
            type="text"
            placeholder="Sektion"
            v-model="itemSection"
            :disabled="isLoading"
          />
        </div>
      </div>

      <!-- Description -->
      <div class="pb-4">
        <label for="edit_news_description">Beskrivelse</label>
        <br />
        <textarea
          id="edit_news_description"
          class="w-full bg-theme-soft border border-theme-border rounded-xl p-2 h-96 text-theme-heading placeholder-theme-muted focus:outline-none focus:ring-2 focus:ring-theme-accent focus:border-theme-accent"
          placeholder="Beskrivelse"
          v-model="itemDescription"
          :disabled="isLoading"
        ></textarea>
      </div>

      <!-- Image replacement (optional) -->
      <div class="pb-4">
        <label for="edit_news_file">Billede (valgfri — erstatter nuværende billede)</label>
        <br />
        <input
          id="edit_news_file"
          class="w-full bg-theme-soft border border-theme-border rounded-xl p-2 cursor-pointer hover:bg-theme-mute text-theme-text focus:outline-none focus:ring-2 focus:ring-theme-accent"
          type="file"
          accept="image/*"
          @change="handleFileUpload"
          :disabled="isLoading"
        />
      </div>

      <div v-if="fileUploadError" class="pb-4 text-red-400">
        <span>Kan ikke uploade en fil med filstørrelse på 0 bytes!</span>
      </div>

      <div v-if="submitError" class="pb-4 text-red-400">
        <span>{{ submitError }}</span>
      </div>

      <div class="mt-4 flex gap-3">
        <button
          type="submit"
          class="cursor-pointer inline-flex justify-center rounded-md border border-transparent bg-amber-600 px-4 py-2 text-md font-semibold hover:bg-amber-500 focus:outline-none focus-visible:ring-2 focus-visible:ring-theme-accent text-navy-950 focus-visible:ring-offset-2 shadow-lg shadow-amber-600/20 disabled:opacity-50"
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
            Opdaterer...
          </span>
          <span v-else>Gem ændringer</span>
        </button>
        <button
          type="button"
          class="cursor-pointer inline-flex justify-center rounded-md border border-transparent bg-theme-mute px-4 py-2 text-md font-medium hover:bg-theme-border hover:text-theme-accent focus:outline-none focus-visible:ring-2 focus-visible:ring-theme-accent text-theme-heading focus-visible:ring-offset-2 disabled:opacity-50"
          :disabled="isLoading"
          @click="emit('close')"
        >
          Annuller
        </button>
      </div>
    </form>
  </BaseModal>
</template>

<style lang="css" scoped></style>
