<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref } from 'vue'
import api from '@/services/apiservice'
import urlservice from '@/services/urlservice'
import type { NexiCheckoutSessionDto } from '@/types/payment'

interface DibsCheckoutOptions {
  checkoutKey: string
  paymentId: string
  containerId: string
  language?: string
  theme?: {
    buttonRadius?: string
    primaryColor?: string
    buttonTextColor?: string
    fontFamily?: string
  }
}

interface DibsCheckout {
  on: (eventName: string, callback: (payload: unknown) => void) => void
  cleanup: () => void
}

interface DibsWindow {
  Dibs?: {
    Checkout: new (options: DibsCheckoutOptions) => DibsCheckout
  }
}

const checkoutContainerId = 'nexi-checkout-container'
const isLoading = ref(true)
const error = ref('')
const paymentId = ref('')
const checkoutInstance = ref<DibsCheckout | null>(null)

function getDibsSdkWindow() {
  return window as unknown as DibsWindow
}

function cleanupCheckout() {
  if (!checkoutInstance.value) return

  checkoutInstance.value.cleanup()
  checkoutInstance.value = null
}

async function loadCheckoutScript(checkoutJsUrl: string) {
  const existingScript = document.querySelector(
    'script[data-nexi-checkout-script="true"]',
  ) as HTMLScriptElement | null

  if (existingScript) {
    if (getDibsSdkWindow().Dibs) return

    await new Promise<void>((resolve, reject) => {
      existingScript.addEventListener('load', () => resolve(), { once: true })
      existingScript.addEventListener('error', () => reject(new Error('Kunne ikke indlæse checkout.')), {
        once: true,
      })
    })
    return
  }

  await new Promise<void>((resolve, reject) => {
    const script = document.createElement('script')
    script.src = checkoutJsUrl
    script.async = true
    script.dataset.nexiCheckoutScript = 'true'
    script.onload = () => resolve()
    script.onerror = () => reject(new Error('Kunne ikke indlæse checkout.'))
    document.head.append(script)
  })
}

async function startCheckout() {
  isLoading.value = true
  error.value = ''

  try {
    const response = await api.post<NexiCheckoutSessionDto>(urlservice.getNexiPocSession(), {}, { skipAuth: true })
    const session = response.data

    paymentId.value = session.paymentId
    await loadCheckoutScript(session.checkoutJsUrl)

    const dibs = getDibsSdkWindow().Dibs
    if (!dibs) {
      throw new Error('Checkout SDK blev ikke initialiseret korrekt.')
    }

    const checkout = new dibs.Checkout({
      checkoutKey: session.checkoutKey,
      paymentId: session.paymentId,
      containerId: checkoutContainerId,
      language: session.language,
      theme: {
        buttonRadius: '10px',
        primaryColor: '#d97706',
        buttonTextColor: '#0f172a',
        fontFamily: 'Lato',
      },
    })

    checkout.on('payment-completed', (event) => {
      console.info('Nexi betaling gennemfoert', event)
    })

    checkoutInstance.value = checkout
  } catch (err: unknown) {
    if (err instanceof Error) {
      error.value = err.message
    } else {
      error.value = 'Der opstod en fejl under opstart af checkout.'
    }
  } finally {
    isLoading.value = false
  }
}

onMounted(async () => {
  await startCheckout()
})

onBeforeUnmount(() => {
  cleanupCheckout()
})
</script>

<template>
  <main class="mx-auto w-full max-w-6xl px-4 py-12 sm:px-6 lg:px-8">
    <section class="rounded-2xl border border-theme-border bg-theme-soft p-6 sm:p-10">
      <header class="mb-8">
        <p class="mb-2 text-xs font-semibold uppercase tracking-[0.24em] text-theme-accent">Betaling</p>
        <h1 class="text-3xl font-bold sm:text-4xl">Nets Easy checkout demo</h1>
        <p class="mt-3 text-sm text-theme-text sm:text-base">
          Dette er en proof-of-concept integration med hardkodede testdata.
        </p>
      </header>

      <div v-if="isLoading" class="rounded-xl border border-theme-border bg-theme-mute p-5 text-sm text-theme-text">
        Vi opretter en testbetaling og loader checkout...
      </div>

      <div v-if="error" class="rounded-xl border border-red-700/60 bg-red-950/40 p-5 text-sm text-red-200">
        <p class="font-semibold">Checkout kunne ikke startes</p>
        <p class="mt-2">{{ error }}</p>
      </div>

      <div
        v-show="!isLoading && !error"
        :id="checkoutContainerId"
        class="min-h-[640px] rounded-xl border border-theme-border bg-white"
      />

      <p v-if="paymentId" class="mt-6 text-xs text-theme-text">PaymentId: {{ paymentId }}</p>
    </section>
  </main>
</template>

<style lang="css" scoped>
main {
  animation: fade-in 0.35s ease-out;
}

@keyframes fade-in {
  from {
    opacity: 0;
    transform: translateY(8px);
  }

  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>
