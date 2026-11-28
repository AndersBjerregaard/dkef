<script setup lang="ts">
import { ref, computed, type Ref } from 'vue'
import { v4 as uuidv4 } from 'uuid'
import type { AxiosResponse } from 'axios'
import apiservice from '@/services/apiservice'
import urlservice from '@/services/urlservice'
import type { AttachmentItem } from '@/types/attachments'
import { getStorageFileError, STORAGE_UPLOAD_HINT, uploadFile } from '@/utils/storageUploads'

const props = defineProps<{
  attachments: AttachmentItem[]
  isLoading: boolean
  contentType: 'events' | 'news' | 'general-assemblies'
}>()

const emit = defineEmits<{
  (e: 'update:attachments', attachments: AttachmentItem[]): void
  (e: 'error', msg: string): void
}>()

const attachmentFiles: Ref<File[]> = ref([])
const uploadErrors: Ref<string[]> = ref([])

const displayAttachments = computed(() => {
  return props.attachments.map((attachment, idx) => ({
    id: attachment.id,
    name: attachment.fileName || `Vedhæftning ${idx + 1}`,
  }))
})

function handleFilesUpload(event: Event) {
  uploadErrors.value = []
  const input = event.target as HTMLInputElement
  if (input.files) {
    const newFiles = Array.from(input.files)
    for (const file of newFiles) {
      const error = getStorageFileError(file)
      if (error) {
        uploadErrors.value.push(error)
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
  emit(
    'update:attachments',
    props.attachments.filter((attachment) => attachment.id !== id),
  )
}

async function processNewAttachments(): Promise<AttachmentItem[]> {
  uploadErrors.value = []
  const newAttachments: AttachmentItem[] = []
  for (const file of [...attachmentFiles.value]) {
    try {
      const newGuid = uuidv4()
      const presignedUrlResponse: AxiosResponse<string> = await apiservice.get<string>(
        urlservice.getAttachmentPresignedUrl(newGuid),
      )
      await uploadFile(presignedUrlResponse.data, file)
      newAttachments.push({
        id: newGuid,
        fileName: file.name,
      })
      attachmentFiles.value.splice(attachmentFiles.value.indexOf(file), 1)
    } catch (err: unknown) {
      const message =
        err instanceof Error ? err.message : `"${file.name}": Filen kunne ikke uploades. Prøv igen.`
      uploadErrors.value = [message]
      // Preserve completed uploads when a later file fails; retry only pending files.
      if (newAttachments.length > 0) {
        emit('update:attachments', [...props.attachments, ...newAttachments])
      }
      emit('error', message)
      throw new Error(message)
    }
  }
  attachmentFiles.value = []
  return newAttachments
}

defineExpose({
  processNewAttachments,
})
</script>

<template>
  <div>
    <!-- File input -->
    <div class="pb-4">
      <label for="attachment_files">Vedhæftelser (valgfrit)</label>
      <p class="text-sm">{{ STORAGE_UPLOAD_HINT }}</p>
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
    <div v-if="uploadErrors.length > 0" class="pb-4 text-red-400" role="alert">
      <div v-for="(error, idx) in uploadErrors" :key="idx">
        <span>{{ error }}</span>
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
