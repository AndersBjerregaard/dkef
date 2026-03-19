<script setup lang="ts">
import { ref, type Ref } from 'vue'
import LoadingButton from '@/components/LoadingButton.vue'
import type { ContactMessageDto } from '@/types/contactMessage'
import apiservice from '@/services/apiservice'
import urlservice from '@/services/urlservice'
import 'vue-sonner/style.css'
import { Toaster, toast } from 'vue-sonner'

const contactName: Ref<string> = ref('')
const contactPhone: Ref<string> = ref('')
const contactEmail: Ref<string> = ref('')
const contactMessage: Ref<string> = ref('')

const isLoading: Ref<boolean> = ref(false)
const submitError: Ref<string | null> = ref(null)

async function submitContactMessage() {
  isLoading.value = true
  submitError.value = null

  try {
    const dto: ContactMessageDto = {
      name: contactName.value,
      phone: contactPhone.value,
      email: contactEmail.value,
      message: contactMessage.value,
    }
    await apiservice.post<unknown>(urlservice.postContactMessage(), dto, { skipAuth: true })
    toast.success('Tak for din besked! Vi svarer tilbage hurtigst muligt.')
    clearForm()
  } catch (err: unknown) {
    const axiosError = err as { response?: { data?: { message?: string } }; message?: string }
    submitError.value =
      axiosError.response?.data?.message || axiosError.message || 'Fejl forsendelse. Prøv igen.'
    console.error(err)
    toast.error('Der skete en fejl i forsendelsen af beskeden.')
  } finally {
    isLoading.value = false
  }
}

function clearForm() {
  contactName.value = ''
  contactPhone.value = ''
  contactEmail.value = ''
  contactMessage.value = ''
  submitError.value = null
}
</script>

<template>
  <Toaster />
  <form @submit.prevent="submitContactMessage">
    <div class="flex-row h-96">
      <label for="name_input">Navn</label>
      <br />
      <input
        class="w-full bg-theme-soft border border-theme-border rounded-xl p-2 focus:outline-none focus:ring-2 focus:ring-theme-accent focus:border-theme-accent"
        id="name_input"
        name="name_input"
        placeholder="Navn"
        type="texit"
        required
        minlength="2"
        v-model="contactName"
        :disabled="isLoading"
      />
      <br />
      <label for="phone_input">Telefon</label>
      <br />
      <input
        class="w-full bg-theme-soft border border-theme-border rounded-xl p-2 focus:outline-none focus:ring-2 focus:ring-theme-accent focus:border-theme-accent"
        id="phone_input"
        name="phone_input"
        placeholder="Telefon"
        type="tel"
        required
        minlength="8"
        v-model="contactPhone"
        :disabled="isLoading"
      />
      <br />
      <label for="email_input">Email</label>
      <br />
      <input
        class="w-full bg-theme-soft border border-theme-border rounded-xl p-2 focus:outline-none focus:ring-2 focus:ring-theme-accent focus:border-theme-accent"
        id="email_input"
        name="email_input"
        placeholder="Email"
        type="email"
        required
        v-model="contactEmail"
        :disabled="isLoading"
      />
      <br />
      <div>
        <label class="text-xl" for="message_input">Besked</label>
        <br />
        <textarea
          class="w-full h-32 bg-theme-soft border border-theme-border rounded-2xl p-2 focus:outline-none focus:ring-2 focus:ring-theme-accent focus:border-theme-accent"
          id="message_input"
          name="message_input"
          placeholder="Skriv din besked her"
          required
          v-model="contactMessage"
          :disabled="isLoading"
        ></textarea>
      </div>
      <br />
      <div v-if="submitError" class="pb-4 text-red-400">
        <span>{{ submitError }}</span>
      </div>
      <div>
        <LoadingButton
          type="submit"
          default-text="Send besked"
          loading-text="Sender..."
          :is-loading="isLoading"
        />
      </div>
    </div>
  </form>
</template>

<style lang="css" scoped></style>
