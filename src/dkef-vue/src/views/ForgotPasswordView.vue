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
      'Failed to send password reset email. Please try again.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
    <div class="max-w-md w-full space-y-8">
      <div>
        <h2 class="mt-6 text-center text-3xl font-extrabold text-gray-900">
          Reset your password
        </h2>
        <p class="mt-2 text-center text-sm text-gray-600">
          Enter your email address and we'll send you a password reset link.
        </p>
      </div>

      <div v-if="success" class="rounded-md bg-green-50 p-4">
        <p class="text-sm text-green-800">
          Password reset instructions have been sent to your email. Please check your inbox and
          follow the link to reset your password.
        </p>
      </div>

      <form v-else class="mt-8 space-y-6" @submit.prevent="handleForgotPassword">
        <div v-if="error" class="rounded-md bg-red-50 p-4">
          <p class="text-sm text-red-800">{{ error }}</p>
        </div>

        <div>
          <label for="email-address" class="block text-sm font-medium text-gray-700">
            Email address
          </label>
          <input
            id="email-address"
            v-model="email"
            name="email"
            type="email"
            autocomplete="email"
            required
            class="mt-1 appearance-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-md focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
            placeholder="Email address"
          />
        </div>

        <div>
          <button
            type="submit"
            :disabled="loading"
            class="group relative w-full flex justify-center py-2 px-4 border border-transparent text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:opacity-50"
          >
            {{ loading ? 'Sending...' : 'Send reset link' }}
          </button>
        </div>

        <div class="text-center text-sm">
          <router-link to="/login" class="font-medium text-indigo-600 hover:text-indigo-500">
            Back to login
          </router-link>
        </div>
      </form>
    </div>
  </div>
</template>
