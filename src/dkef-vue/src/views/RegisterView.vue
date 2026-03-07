<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import type { RegisterDto } from '@/types/auth'

const router = useRouter()
const authStore = useAuthStore()

const email = ref('')
const password = ref('')
const confirmPassword = ref('')
const firstName = ref('')
const lastName = ref('')
const error = ref('')
const loading = ref(false)

async function handleRegister() {
  error.value = ''

  // Validation
  if (password.value !== confirmPassword.value) {
    error.value = 'Adgangskodeordene er ikke ens'
    return
  }

  if (password.value.length < 8) {
    error.value = 'Adgangskoden skal være mindst 8 tegn lang'
    return
  }

  loading.value = true

  try {
    const registrationData: RegisterDto = {
      email: email.value,
      password: password.value,
      confirmPassword: confirmPassword.value,
      firstName: firstName.value,
      lastName: lastName.value,
    }

    await authStore.register(registrationData)
    router.push('/')
  } catch (err) {
    const axiosError = err as { response?: { data?: { message?: string } }; message?: string }
    error.value =
      axiosError.response?.data?.message ||
      axiosError.message ||
      'Registrering mislykkedes. Prøv igen.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="min-h-screen flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8">
    <div class="max-w-md w-full space-y-6">
      <div>
        <h2 class="mt-6 text-center text-3xl font-bold">Opret din konto</h2>
      </div>
      <form class="mt-8 space-y-4" @submit.prevent="handleRegister">
        <div v-if="error" class="rounded-xl bg-red-900 bg-opacity-50 p-4 border border-red-700">
          <p class="text-sm text-red-200">{{ error }}</p>
        </div>

        <div class="space-y-4">
          <div>
            <label for="first-name" class="block text-sm font-medium mb-1"> Fornavn </label>
            <input
              id="first-name"
              v-model="firstName"
              name="first-name"
              type="text"
              required
              class="w-full bg-gray-800 border-0 rounded-xl p-2 text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-gray-600"
              placeholder="Fornavn"
            />
          </div>

          <div>
            <label for="last-name" class="block text-sm font-medium mb-1"> Efternavn </label>
            <input
              id="last-name"
              v-model="lastName"
              name="last-name"
              type="text"
              required
              class="w-full bg-gray-800 border-0 rounded-xl p-2 text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-gray-600"
              placeholder="Efternavn"
            />
          </div>

          <div>
            <label for="email-address" class="block text-sm font-medium mb-1"> Email </label>
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

          <div>
            <label for="password" class="block text-sm font-medium mb-1">Adgangskode</label>
            <input
              id="password"
              v-model="password"
              name="password"
              type="password"
              autocomplete="new-password"
              required
              class="w-full bg-gray-800 border-0 rounded-xl p-2 text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-gray-600"
              placeholder="Adgangskode (min 8 tegn)"
            />
          </div>

          <div>
            <label for="confirm-password" class="block text-sm font-medium mb-1">
              Bekræft adgangskode
            </label>
            <input
              id="confirm-password"
              v-model="confirmPassword"
              name="confirm-password"
              type="password"
              autocomplete="new-password"
              required
              class="w-full bg-gray-800 border-0 rounded-xl p-2 text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-gray-600"
              placeholder="Bekræft adgangskode"
            />
          </div>
        </div>

        <div class="pt-4">
          <button
            type="submit"
            :disabled="loading"
            class="w-full flex justify-center items-center rounded-xl bg-gray-600 h-12 px-8 cursor-pointer hover:bg-gray-800 text-lg disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            {{ loading ? 'Opretter konto...' : 'Opret konto' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>
