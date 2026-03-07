<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import type { ResetPasswordDto } from '@/types/auth'

const route = useRoute()
const authStore = useAuthStore()

const password = ref('')
const confirmPassword = ref('')
const token = ref('')
const success = ref(false)
const error = ref('')
const loading = ref(false)

onMounted(() => {
  // Get token from query parameters
  token.value = (route.query.token as string) || ''

  if (!token.value) {
    error.value = 'Ugyldig eller manglende token til nulstilling af adgangskode.'
  }
})

async function handleResetPassword() {
  error.value = ''

  // Validation
  if (password.value !== confirmPassword.value) {
    error.value = 'Adgangskoderne matcher ikke'
    return
  }

  if (password.value.length < 8) {
    error.value = 'Adgangskoden skal være mindst 8 tegn lang'
    return
  }

  loading.value = true

  try {
    const resetData: ResetPasswordDto = {
      token: token.value,
      newPassword: password.value,
    }

    await authStore.resetPassword(resetData)
    success.value = true
  } catch (err) {
    const axiosError = err as { response?: { data?: { message?: string } }; message?: string }
    error.value =
      axiosError.response?.data?.message ||
      axiosError.message ||
      'Kunne ikke nulstille adgangskoden. Prøv igen.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="min-h-screen flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8">
    <div class="max-w-md w-full space-y-6">
      <div>
        <h2 class="mt-6 text-center text-3xl font-bold">Ny adgangskode</h2>
      </div>

      <div v-if="success" class="rounded-xl bg-green-900 bg-opacity-50 p-4 border border-green-700">
        <p class="text-sm text-green-200">
          Din adgangskode er blevet nulstillet! Du kan nu logge ind med din nye adgangskode.
        </p>
      </div>

      <form v-else class="mt-8 space-y-4" @submit.prevent="handleResetPassword">
        <div v-if="error" class="rounded-xl bg-red-900 bg-opacity-50 p-4 border border-red-700">
          <p class="text-sm text-red-200">{{ error }}</p>
        </div>

        <div class="space-y-4">
          <div>
            <label for="password" class="block text-sm font-medium mb-1"> Ny adgangskode </label>
            <input
              id="password"
              v-model="password"
              name="password"
              type="password"
              autocomplete="new-password"
              required
              class="w-full bg-gray-800 border-0 rounded-xl p-2 text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-gray-600"
              placeholder="Ny adgangskode (min 8 tegn)"
            />
          </div>

          <div>
            <label for="confirm-password" class="block text-sm font-medium mb-1">
              Bekræft ny adgangskode
            </label>
            <input
              id="confirm-password"
              v-model="confirmPassword"
              name="confirm-password"
              type="password"
              autocomplete="new-password"
              required
              class="w-full bg-gray-800 border-0 rounded-xl p-2 text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-gray-600"
              placeholder="Bekræft ny adgangskode"
            />
          </div>
        </div>

        <div class="pt-4">
          <button
            type="submit"
            :disabled="loading || !token"
            class="w-full flex justify-center items-center rounded-xl bg-gray-600 h-12 px-8 cursor-pointer hover:bg-gray-800 text-lg disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            {{ loading ? 'Nulstiller adgangskode...' : 'Nulstil adgangskode' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>
