<script setup lang="ts">
import { onMounted, ref, type Ref } from 'vue'
import { useRoute } from 'vue-router'
import api from '@/services/apiservice'

const route = useRoute()

const success: Ref<boolean> = ref(false)
const isLoading: Ref<boolean> = ref(true)
const token: Ref<string> = ref('')
const error: Ref<string> = ref('')

onMounted(async () => {
  token.value = (route.query.token as string) || ''

  if (!token.value) {
    error.value = 'Linket til annullering af e-mail adresse ændring er ugyldigt.'
    success.value = false
    isLoading.value = false
    return
  }

  try {
    await api.post('/profile/change-email/revoke/' + token.value)
    success.value = true
  } catch (err) {
    success.value = false
    const axiosError = err as { response?: { data?: { message?: string } }; message?: string }
    console.error(axiosError)
    error.value = 'En fejl opstod ved annulleringen af e-mail adresse ændringen.'
  } finally {
    isLoading.value = false
  }
})
</script>

<template>
  <main>
    <div class="pt-24">
      <h2 class="mt-6 text-center text-3xl font-bold">Annuller e-mail addresse ændring</h2>
    </div>

    <div class="flex justify-center">
      <!-- Loading spinner -->
      <div v-if="isLoading" class="flex justify-center items-center min-h-[200px]">
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
        <span class="ml-3 text-lg">Bekræfter...</span>
      </div>

      <!-- Main section -->
      <div v-else class="max-w-md w-full space-y-6 py-12">
        <div v-if="success">
          <div class="rounded-xl bg-green-900 bg-opacity-50 p-4 border border-green-700">
            <p class="text-sm text-green-200">
              E-mail adresse ændringen er nu annulleret. For din egen sikkerhed anbefaler vi også at
              ændre din adgangskode.
            </p>
          </div>
          <div class="flex justify-center pt-8">
            <RouterLink to="/">
              <button
                class="rounded-lg bg-amber-600 h-12 py-2 px-6 cursor-pointer hover:bg-amber-500 active:bg-amber-700 text-navy-950 font-semibold text-base shadow-lg shadow-amber-600/20 transition-all"
              >
                Gå til forsiden
              </button>
            </RouterLink>
          </div>
        </div>

        <div v-else>
          <p class="rounded-xl bg-red-900 bg-opacity-50 p-4 border border-red-700">
            {{ error }}
          </p>
        </div>
      </div>
    </div>
  </main>
</template>
