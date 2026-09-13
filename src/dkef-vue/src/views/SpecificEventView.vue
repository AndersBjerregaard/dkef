<script setup lang="ts">
import { useEventStore } from '@/stores/eventStore'
import { useAuthStore } from '@/stores/authStore'
import { type EventSignUpListItem, type PublishedEvent } from '@/types/events'
import { computed, onMounted, ref, watch } from 'vue'
import EditEventModal from '@/components/EditEventModal.vue'
import DeleteEventModal from '@/components/DeleteEventModal.vue'
import { toast } from 'vue-sonner'

const props = defineProps({
  id: {
    type: String,
    required: true,
  },
})

const eventStore = useEventStore()
const authStore = useAuthStore()

const currentEvent = computed<PublishedEvent | undefined>(() => eventStore.getEventById(props.id))

const isEditOpen = ref(false)
const isDeleteOpen = ref(false)
const isSignedUp = ref(false)
const signedUpAt = ref<string | null>(null)
const isSignUpLoading = ref(false)
const signUpError = ref<string | null>(null)
const attendees = ref<EventSignUpListItem[]>([])
const isAttendeesLoading = ref(false)

const dateTime = computed(() => {
  const event = currentEvent.value
  if (event === undefined) {
    return ''
  }
  const date: Date = new Date(event!.dateTime)
  return new Intl.DateTimeFormat(undefined, {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: 'numeric',
    minute: 'numeric',
  }).format(date)
})

const signUpDeadline = computed(() => {
  const event = currentEvent.value
  if (!event?.signUpDeadline) {
    return ''
  }

  const date = new Date(event.signUpDeadline)
  return new Intl.DateTimeFormat(undefined, {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: 'numeric',
    minute: 'numeric',
  }).format(date)
})

const hasEventStarted = computed(() => {
  const event = currentEvent.value
  if (!event) {
    return false
  }
  return new Date(event.dateTime).getTime() <= Date.now()
})

const isSignUpDeadlinePassed = computed(() => {
  const event = currentEvent.value
  if (!event?.signUpDeadline) {
    return false
  }

  return new Date(event.signUpDeadline).getTime() < Date.now()
})

const canSignUp = computed(() => {
  return !isSignedUp.value && !hasEventStarted.value && !isSignUpDeadlinePassed.value
})

const signedUpAtLabel = computed(() => {
  if (!signedUpAt.value) {
    return ''
  }

  return new Intl.DateTimeFormat(undefined, {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: 'numeric',
    minute: 'numeric',
  }).format(new Date(signedUpAt.value))
})

async function loadEvent(id: string): Promise<void> {
  await eventStore.fetchEvent(id)
}

async function loadMySignUpStatus(): Promise<void> {
  if (!authStore.isAuthenticated) {
    isSignedUp.value = false
    signedUpAt.value = null
    return
  }

  try {
    const status = await eventStore.fetchMySignUpStatus(props.id)
    isSignedUp.value = status.isSignedUp
    signedUpAt.value = status.signedUpAt ?? null
  } catch (error) {
    console.error('Failed to fetch sign-up status', error)
  }
}

async function loadAttendees(): Promise<void> {
  if (!authStore.isAdmin) {
    attendees.value = []
    return
  }

  isAttendeesLoading.value = true
  try {
    const result = await eventStore.fetchEventSignUps(props.id)
    attendees.value = result.collection
  } catch (error) {
    console.error('Failed to fetch attendees', error)
  } finally {
    isAttendeesLoading.value = false
  }
}

function promptLoginForSignUp() {
  window.dispatchEvent(
    new CustomEvent<{ redirectPath?: string }>('auth:prompt-login', {
      detail: {
        redirectPath: window.location.pathname + window.location.search,
      },
    }),
  )
}

async function signUpForEvent() {
  signUpError.value = null
  if (!authStore.isAuthenticated) {
    promptLoginForSignUp()
    return
  }

  if (!canSignUp.value) {
    return
  }

  isSignUpLoading.value = true
  try {
    const response = await eventStore.signUpForEvent(props.id)
    isSignedUp.value = true
    signedUpAt.value = response.signedUpAt
    toast.success('Du er nu tilmeldt arrangementet')
    await loadAttendees()
  } catch (error: unknown) {
    const errorMessage =
      error && typeof error === 'object' && 'response' in error
        ? (error as { response?: { data?: string | { message?: string } } }).response?.data
        : undefined

    const parsedError =
      typeof errorMessage === 'string'
        ? errorMessage
        : errorMessage && typeof errorMessage === 'object' && 'message' in errorMessage
          ? (errorMessage.message as string)
          : 'Kunne ikke tilmelde arrangementet. Prøv igen.'

    signUpError.value = parsedError
    toast.error('Tilmelding fejlede', {
      description: parsedError,
    })
  } finally {
    isSignUpLoading.value = false
  }
}

onMounted(async () => {
  await loadEvent(props.id)
  await loadMySignUpStatus()
  await loadAttendees()
})

watch(
  () => authStore.isAuthenticated,
  async () => {
    await loadMySignUpStatus()
    await loadAttendees()
  },
)
</script>

<template>
  <div class="py-16 w-screen px-8 justify-items-center">
    <div class="w-[70%]">
      <div class="pb-4 flex gap-4">
        <RouterLink
          to="/events-and-news"
          class="flex justify-center items-center rounded-lg bg-theme-mute h-14 w-64 p-2 cursor-pointer hover:bg-theme-border hover:text-theme-accent transition-colors text-theme-heading"
        >
          &larr; Tilbage til arrangementer og nyheder
        </RouterLink>
        <button
          v-if="authStore.isAdmin && currentEvent"
          class="flex justify-center items-center rounded-lg bg-theme-mute h-14 w-36 p-2 cursor-pointer hover:bg-theme-border hover:text-theme-accent transition-colors text-theme-heading font-bold"
          @click="isEditOpen = true"
        >
          Rediger
        </button>
        <button
          v-if="authStore.isAdmin"
          class="flex justify-center items-center rounded-lg bg-red-400 h-14 w-36 p-2 cursor-pointer hover:bg-red-900 hover:text-theme-accent transition-colors text-theme-heading font-bold"
          @click="isDeleteOpen = true"
        >
          Slet
        </button>
      </div>

      <!-- Loading -->
      <div v-if="eventStore.isFetching" class="flex justify-center items-center min-h-[200px]">
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

      <!-- Error -->
      <div v-else-if="eventStore.error">
        <p class="text-4xl">{{ eventStore.error }}</p>
      </div>

      <!-- Event -->
      <div v-else-if="currentEvent">
        <div class="text-4xl pb-8">
          <h1>{{ currentEvent?.title }}</h1>
        </div>
        <div class="text-xl">
          <div class="pb-4">
            <h2>{{ currentEvent.section }}</h2>
          </div>
          <div class="pb-4">
            <h2>{{ currentEvent.address }}</h2>
          </div>
          <div class="pb-4">
            <h2>{{ dateTime }}</h2>
          </div>
          <div v-if="signUpDeadline" class="pb-4">
            <h2>Tilmeldingsfrist: {{ signUpDeadline }}</h2>
          </div>
        </div>

        <div class="pb-8">
          <div class="rounded-lg border border-theme-border bg-theme-mute p-4">
            <p class="font-semibold pb-2">Tilmelding</p>

            <p v-if="isSignedUp" class="text-emerald-400">
              Du er tilmeldt arrangementet<span v-if="signedUpAtLabel"> ({{ signedUpAtLabel }})</span>.
            </p>
            <p v-else-if="hasEventStarted" class="text-theme-text">
              Arrangementet er allerede afholdt.
            </p>
            <p v-else-if="isSignUpDeadlinePassed" class="text-theme-text">
              Tilmeldingsfristen er udløbet.
            </p>
            <p v-else class="text-theme-text pb-3">
              Tilmeld dig arrangementet her.
            </p>

            <button
              v-if="!isSignedUp"
              type="button"
              class="rounded-lg bg-amber-600 h-11 px-4 cursor-pointer hover:bg-amber-500 active:bg-amber-700 text-navy-950 font-semibold transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
              :disabled="isSignUpLoading || (authStore.isAuthenticated && !canSignUp)"
              @click="signUpForEvent"
            >
              {{
                isSignUpLoading
                  ? 'Tilmelder...'
                  : authStore.isAuthenticated
                    ? 'Tilmeld'
                    : 'Log ind for at tilmelde'
              }}
            </button>

            <p v-if="signUpError" class="text-red-400 pt-3">{{ signUpError }}</p>
          </div>
        </div>

        <div class="flex items-center justify-center h-full">
          <div class="h-60 w-60 pb-4 flex items-center justify-center">
            <img
              class="max-w-full max-h-full object-contain"
              :src="currentEvent.thumbnailUrl"
              alt="event picture"
            />
          </div>
        </div>
        <div class="whitespace-pre-wrap">
          {{ currentEvent.description }}
        </div>
        <div v-if="currentEvent.attachmentUrls.length > 0" class="pt-8">
          <h3 class="text-2xl pb-3">Vedhæftninger</h3>
          <ul class="space-y-2">
            <li
              v-for="(attachmentUrl, index) in currentEvent.attachmentUrls"
              :key="`${attachmentUrl}-${index}`"
            >
              <a
                :href="attachmentUrl"
                target="_blank"
                rel="noopener noreferrer"
                class="underline break-all text-theme-heading hover:text-theme-accent transition-colors"
              >
                {{ currentEvent.attachmentFileNames?.[index] || `Vedhæftning ${index + 1}` }}
              </a>
            </li>
          </ul>
        </div>

        <div v-if="authStore.isAdmin" class="pt-10">
          <h3 class="text-2xl pb-3">Tilmeldte medlemmer ({{ attendees.length }})</h3>

          <div v-if="isAttendeesLoading" class="text-theme-text">Henter tilmeldinger...</div>

          <div
            v-else-if="attendees.length === 0"
            class="rounded-lg border border-theme-border bg-theme-mute p-4 text-theme-text"
          >
            Der er ingen tilmeldte endnu.
          </div>

          <div v-else class="overflow-x-auto">
            <table class="w-full border border-theme-border rounded-lg overflow-hidden">
              <thead class="bg-theme-mute">
                <tr>
                  <th class="text-left p-3">Navn</th>
                  <th class="text-left p-3">Email</th>
                  <th class="text-left p-3">Tilmeldt</th>
                </tr>
              </thead>
              <tbody>
                <tr
                  v-for="attendee in attendees"
                  :key="`${attendee.contactId}-${attendee.signedUpAt}`"
                  class="border-t border-theme-border"
                >
                  <td class="p-3">{{ attendee.name }}</td>
                  <td class="p-3">{{ attendee.email }}</td>
                  <td class="p-3">
                    {{
                      new Intl.DateTimeFormat(undefined, {
                        year: 'numeric',
                        month: 'long',
                        day: 'numeric',
                        hour: 'numeric',
                        minute: 'numeric',
                      }).format(new Date(attendee.signedUpAt))
                    }}
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>

      <!-- Not Found -->
      <div v-else>
        <p class="text-4xl">Ikke fundet</p>
      </div>
    </div>
  </div>

  <EditEventModal
    v-if="currentEvent"
    :is-open="isEditOpen"
    :event="currentEvent"
    @close="isEditOpen = false"
  />
  <DeleteEventModal
    v-if="currentEvent"
    :is-open="isDeleteOpen"
    :event="currentEvent"
    @close="isDeleteOpen = false"
  />
</template>

<style lang="css" scoped></style>
