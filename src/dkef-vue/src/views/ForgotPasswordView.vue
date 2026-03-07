<script setup lang="ts">
import { ref } from 'vue'
import { useAuthStore } from '@/stores/authStore'

const authStore = useAuthStore()

const email = ref('')
const success = ref(false)
const error = ref('')
const loading = ref(false)

async function handleForgotPassword() {
  error.value = ''
  loading.value = true

  try {
    await authStore.forgotPassword(email.value)
    success.value = true
  } catch (err) {
    const axiosError = err as { response?: { data?: { message?: string } }; message?: string }
    error.value =
      axiosError.response?.data?.message ||
      axiosError.message ||
      'Fejl ved afsendelse af nulstillings-e-mail. Prøv igen.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="min-h-screen flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8">
    <div class="max-w-md w-full space-y-6">
      <div>
        <h2 class="mt-6 text-center text-3xl font-bold">Nulstil din adgangskode</h2>
        <p class="mt-2 text-center text-sm">
          Indtast din e-mailadresse, og vi sender dig et link til at nulstille din adgangskode.
        </p>
      </div>

      <div v-if="success" class="rounded-xl bg-green-900 bg-opacity-50 p-4 border border-green-700">
        <p class="text-sm text-green-200">
          Instruktioner til nulstilling af adgangskode er blevet sendt til din e-mail. Tjek din
          indbakke og følg linket for at nulstille din adgangskode.
        </p>
      </div>

      <form v-else class="mt-8 space-y-4" @submit.prevent="handleForgotPassword">
        <div v-if="error" class="rounded-xl bg-red-900 bg-opacity-50 p-4 border border-red-700">
          <p class="text-sm text-red-200">{{ error }}</p>
        </div>

        <div>
          <label for="email-address" class="block text-sm font-medium mb-1 py-2"> Email </label>
          <input
            id="email-address"
            v-model="email"
            name="email"
            type="email"
            autocomplete="email"
            required
            class="w-full bg-gray-800 border-0 rounded-xl p-2 text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-gray-600"
            placeholder="Email"
          />
        </div>

        <div class="pt-4">
          <button
            type="submit"
            :disabled="loading"
            class="w-full flex justify-center items-center rounded-xl bg-gray-600 h-12 px-8 cursor-pointer hover:bg-gray-800 text-lg disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            {{ loading ? 'Sender...' : 'Send nulstillingslink' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>
