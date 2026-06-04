<script setup lang="ts">
import { ref, watch, computed, type Ref } from 'vue'
import { v4 as uuidv4 } from 'uuid'
import axios from 'axios'
import type { AxiosResponse } from 'axios'
import apiservice from '@/services/apiservice'
import urlservice from '@/services/urlservice'

const props = defineProps<{
  attachmentIds: string[]
  isLoading: boolean
  contentType: 'events' | 'news' | 'general-assemblies'
}>()

const emit = defineEmits<{
  (e: 'update:attachmentIds', ids: string[]): void
  (e: 'error', msg: string): void
}>()

const attachmentFiles: Ref<File[]> = ref([])
const uploadErrors: Ref<string[]> = ref([])

const displayAttachments = computed(() => {
  return props.attachmentIds.map((id, idx) => ({
    id,
    name: `Attachment ${idx + 1}`,
  }))
})

function handleFilesUpload(event: Event) {
  uploadErrors.value = []
  const input = event.target as HTMLInputElement
  if (input.files) {
    const newFiles = Array.from(input.files)
    for (const file of newFiles) {
      if (file.size === 0) {
        uploadErrors.value.push(`"${file.name}": 0 bytes`)
        continue
      }
      attachmentFiles.value.push(file)
    }
  }
  input.value = ''
}

function removeLocalFile(idx: number) {
  attachmentFiles.value.splice(idx, 1)
}

function removeAttachment(id: string) {
  emit('update:attachmentIds', props.attachmentIds.filter(aid => aid !== id))
}

async function uploadFile(url: string, file: File): Promise<void> {
  const axiosInstance = axios.create()
  await axiosInstance.put(url, file, { headers: { 'Content-Type': file.type } })
}

async function processNewAttachments(): Promise<string[]> {
  const newIds: string[] = []
  for (const file of attachmentFiles.value) {
    try {
      const newGuid = uuidv4()
      const presignedUrlResponse: AxiosResponse<string> = await apiservice.get<string>(
        urlservice.getAttachmentPresignedUrl(newGuid),
      )
      await uploadFile(presignedUrlResponse.data, file)
      newIds.push(newGuid)
    } catch (err: unknown) {
      const axiosError = err as { response?: { data?: { message?: string } }; message?: string }
      emit(
        'error',
        `Failed to upload "${file.name}": ${axiosError.response?.data?.message || axiosError.message}`,
      )
    }
  }
  attachmentFiles.value = []
  return newIds
}

watch(
  () => props.attachmentIds,
  () => {
    uploadErrors.value = []
  },
)

defineExpose({
  processNewAttachments,
})
</script>

<template>
  <div>
    <!-- File input -->
    <div class="pb-4">
      <label for="attachment_files">Vedhæftelser (valgfrit)</label>
      <br />
      <input
        id="attachment_files"
        class="w-full bg-theme-soft border border-theme-border rounded-xl p-2 cursor-pointer hover:bg-theme-mute text-theme-text focus:outline-none focus:ring-2 focus:ring-theme-accent"
        type="file"
        multiple
        accept="*"
        @change="handleFilesUpload"
        :disabled="isLoading"
      />
    </div>

    <!-- Upload errors -->
    <div v-if="uploadErrors.length > 0" class="pb-4 text-red-400">
      <div v-for="(error, idx) in uploadErrors" :key="idx">
        <span>Kan ikke uploade fil med 0 bytes: {{ error }}</span>
      </div>
    </div>

    <!-- Staged files (to be uploaded) -->
    <div v-if="attachmentFiles.length > 0" class="pb-4">
      <div class="text-theme-text text-sm mb-2">Nye filer (klar til upload):</div>
      <div class="space-y-2">
        <div
          v-for="(file, idx) in attachmentFiles"
          :key="`new-${idx}`"
          class="flex items-center justify-between bg-theme-soft p-3 rounded-lg border border-theme-border"
        >
          <span class="text-theme-text">{{ file.name }}</span>
          <button
            type="button"
            class="text-red-400 hover:text-red-500"
            @click="removeLocalFile(idx)"
            :disabled="isLoading"
          >
            ✕
          </button>
        </div>
      </div>
    </div>

    <!-- Existing attachments -->
    <div v-if="displayAttachments.length > 0" class="pb-4">
      <div class="text-theme-text text-sm mb-2">Uploadede vedhæftelser:</div>
      <div class="space-y-2">
        <div
          v-for="att in displayAttachments"
          :key="att.id"
          class="flex items-center justify-between bg-theme-soft p-3 rounded-lg border border-theme-border"
        >
          <span class="text-theme-text">{{ att.name }}</span>
          <button
            type="button"
            class="text-red-400 hover:text-red-500"
            @click="removeAttachment(att.id)"
            :disabled="isLoading"
          >
            ✕
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style lang="css" scoped></style>
