<script setup lang="ts">
import { RouterView, RouterLink } from 'vue-router'
import {
  Menu, MenuButton, MenuItems, MenuItem,
  TransitionRoot, TransitionChild,
  Dialog, DialogPanel, DialogTitle
} from '@headlessui/vue'
import { ref } from 'vue'
import { useAuthStore } from '@/stores/authStore'

const authStore = useAuthStore()
const isOpen = ref(false)
const email = ref('')
const password = ref('')
const loginError = ref('')
const isLoggingIn = ref(false)

function closeModal() {
  isOpen.value = false
  loginError.value = ''
  email.value = ''
  password.value = ''
}

function openModal() {
  isOpen.value = true
}

async function handleLogin() {
  if (!email.value || !password.value) {
    loginError.value = 'Udfyld venligst alle felter'
    return
  }

  isLoggingIn.value = true
  loginError.value = ''

  try {
    await authStore.login({
      email: email.value,
      password: password.value
    })
    closeModal()
  } catch (error: unknown) {
    const errorMessage = error && typeof error === 'object' && 'response' in error
      ? (error as { response?: { data?: { message?: string } } }).response?.data?.message
      : undefined
    loginError.value = errorMessage || 'Login fejlede. Tjek dine oplysninger og prøv igen.'
  } finally {
    isLoggingIn.value = false
  }
}

async function handleLogout() {
  try {
    await authStore.logout()
  } catch (error) {
    console.error('Logout error:', error)
  }
}
</script>

<template>
  <header>
    <div>
      <nav class="flex bg-gray-700 p-1 sm:p-4">
        <RouterLink to="/">
          <div class="w-10 sm:w-20">
            <img src="@/assets/dkef-logo.png" alt="DKEF logo">
          </div>
        </RouterLink>

        <!-- Inline buttons (visible on larger screens ) -->
        <div class="hidden lg:flex lg:w-full">
          <div class="flex p-4 w-full justify-end items-center">
            <div class="p-3" v-if="authStore.isAdmin">
              <RouterLink to="/members">
                <button class="rounded bg-gray-600 h-12 w-36 p-2 cursor-pointer hover:bg-gray-800">Medlemmer</button>
              </RouterLink>
            </div>
            <div class="p-3">
              <RouterLink to="/advantages">
                <button
                  class="rounded bg-gray-600 h-12 w-36 p-2 cursor-pointer hover:bg-gray-800">Medlemsfordele</button>
              </RouterLink>
            </div>
            <div class="p-3">
              <RouterLink to="/events-and-news">
                <button class="rounded bg-gray-600 h-12 w-52 p-2 cursor-pointer hover:bg-gray-800">Arrangementer og
                  nyheder</button>
              </RouterLink>
            </div>
            <div class="p-3">
              <RouterLink to="/about">
                <button class="rounded bg-gray-600 h-12 w-32 p-2 cursor-pointer hover:bg-gray-800">Om
                  foreningen</button>
              </RouterLink>
            </div>
            <div class="p-3">
              <RouterLink to="/contact">
                <button class="rounded bg-gray-600 h-12 w-24 p-2 cursor-pointer hover:bg-gray-800">Kontakt os</button>
              </RouterLink>
            </div>
            <div class="p-3" v-if="!authStore.isAuthenticated">
              <button class="rounded bg-gray-600 h-12 w-20 p-2 cursor-pointer hover:bg-gray-800" @click="openModal">
                Log på
              </button>
            </div>
            <div class="p-3 flex items-center gap-3" v-else>
              <span class="text-white">Hej, {{ authStore.user?.firstName }}</span>
              <button class="rounded bg-gray-600 h-12 w-24 p-2 cursor-pointer hover:bg-gray-800" @click="handleLogout">
                Log ud
              </button>
            </div>
          </div>
        </div>
        <TransitionRoot appear :show="isOpen" as="template">
          <Dialog as="div" @close="closeModal" class="relative z-10">
            <TransitionChild as="template" enter="duration-300 ease-out" enter-from="opacity-0" enter-to="opacity-100"
              leave="duration-200 ease-in" leave-from="opacity-100" leave-to="opacity-0">
              <div class="fixed inset-0 bg-black/25"></div>
            </TransitionChild>

            <div class="fixed inset-0 overflow-y-auto">
              <div class="flex min-h-full items-center justify-center p-4 text-center">
                <TransitionChild as="template" enter="duration-300 ease-out" enter-from="opacity-0 scale-95"
                  enter-to="opacity-100 scale-100" leave="duration-200 ease-in" leave-from="opacity-100 scale-100"
                  leave-to="opacity-0 scale-95">
                  <DialogPanel
                    class="w-full max-w-md transform overflow-hidden rounded-2xl bg-white p-6 text-left align-middle shadow-xl transition-all">
                    <DialogTitle as="h3" class="text-lg font-medium leading-6 text-gray-900">
                      Log på
                    </DialogTitle>
                    <form @submit.prevent="handleLogin" class="mt-4">
                      <div class="mb-4">
                        <label for="email" class="block text-sm font-medium text-gray-700 mb-2">
                          Email
                        </label>
                        <input
                          id="email"
                          v-model="email"
                          type="email"
                          required
                          class="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500 text-gray-900"
                          placeholder="din@email.dk"
                        />
                      </div>
                      <div class="mb-4">
                        <label for="password" class="block text-sm font-medium text-gray-700 mb-2">
                          Adgangskode
                        </label>
                        <input
                          id="password"
                          v-model="password"
                          type="password"
                          required
                          class="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500 text-gray-900"
                          placeholder="••••••••"
                        />
                      </div>

                      <div v-if="loginError" class="mb-4 p-3 bg-red-100 border border-red-400 text-red-700 rounded">
                        {{ loginError }}
                      </div>

                      <div class="flex gap-3">
                        <button
                          type="submit"
                          :disabled="isLoggingIn"
                          class="inline-flex justify-center rounded-md border border-transparent bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 focus-visible:ring-offset-2 cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                          {{ isLoggingIn ? 'Logger på...' : 'Log på' }}
                        </button>
                        <button
                          type="button"
                          class="inline-flex justify-center rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 focus-visible:ring-offset-2 cursor-pointer"
                          @click="closeModal"
                        >
                          Annuller
                        </button>
                      </div>
                    </form>
                  </DialogPanel>
                </TransitionChild>
              </div>
            </div>
          </Dialog>
        </TransitionRoot>

        <!-- Burger menu (visible on small screens) -->
        <div class="lg:hidden flex w-full sm:p-4 justify-end items-center">
          <div class="text-right">
            <Menu>
              <MenuButton
                class="text-lg sm:text-2xl rounded bg-gray-600 h-8 sm:h-12 w-10 sm:w-20 sm:p-2 cursor-pointer hover:bg-gray-800">
                ≡</MenuButton>
              <transition enter-active-class="transition duration-100 ease-out"
                enter-from-class="transform scale-95 opacity-0" enter-to-class="transform scale-100 opacity-100"
                leave-active-class="transition duration-75 ease-in" leave-from-class="transform scale-100 opacity-100"
                leave-to-class="transform scale-95 opacity-0">
                <MenuItems
                  class="absolute right-8 mt-2 w-56 origin-top-right divide-y divide-gray-600 rounded-md bg-gray-700 shadow-lg ring-1 ring-black/5 focus:outline-none">
                  <div class="px-1 py-1">
                    <MenuItem v-if="authStore.isAdmin" v-slot="{ active, close }">
                    <RouterLink to="/members">
                      <button
                        :class="[active ? 'bg-gray-800' : 'bg-gray-600', 'group flex w-full items-center rounded-md px-2 py-2 text-sm cursor-pointer']"
                        @click="close">
                        Medlemmer (admin)
                      </button>
                    </RouterLink>
                    </MenuItem>
                    <MenuItem v-slot="{ active, close }">
                    <RouterLink to="/advantages">
                      <button
                        :class="[active ? 'bg-gray-800' : 'bg-gray-600', 'group flex w-full items-center rounded-md px-2 py-2 text-sm cursor-pointer']"
                        @click="close">
                        Medlemsfordele
                      </button>
                    </RouterLink>
                    </MenuItem>
                    <MenuItem v-slot="{ active, close }">
                    <RouterLink to="/events-and-news">
                      <button
                        :class="[active ? 'bg-gray-800' : 'bg-gray-600', 'group flex w-full items-center rounded-md px-2 py-2 text-sm cursor-pointer']"
                        @click="close">
                        Arrangementer og nyheder
                      </button>
                    </RouterLink>
                    </MenuItem>
                    <MenuItem v-slot="{ active, close }">
                    <RouterLink to="/about">
                      <button
                        :class="[active ? 'bg-gray-800' : 'bg-gray-600', 'group flex w-full items-center rounded-md px-2 py-2 text-sm cursor-pointer']"
                        @click="close">
                        Om foreningen
                      </button>
                    </RouterLink>
                    </MenuItem>
                    <MenuItem v-slot="{ active, close }">
                    <RouterLink to="/contact">
                      <button
                        :class="[active ? 'bg-gray-800' : 'bg-gray-600', 'group flex w-full items-center rounded-md px-2 py-2 text-sm cursor-pointer']"
                        @click="close">
                        Kontakt os
                      </button>
                    </RouterLink>
                    </MenuItem>
                    <MenuItem v-if="!authStore.isAuthenticated" v-slot="{ active }">
                    <button
                      :class="[active ? 'bg-gray-800' : 'bg-gray-600', 'group flex w-full items-center rounded-md px-2 py-2 text-sm cursor-pointer']"
                      @click="openModal">
                      Log på
                    </button>
                    </MenuItem>
                    <MenuItem v-else v-slot="{ active }">
                    <button
                      :class="[active ? 'bg-gray-800' : 'bg-gray-600', 'group flex w-full items-center rounded-md px-2 py-2 text-sm cursor-pointer']"
                      @click="handleLogout">
                      Log ud ({{ authStore.user?.firstName }})
                    </button>
                    </MenuItem>
                  </div>
                </MenuItems>
              </transition>
            </Menu>
          </div>
        </div>
      </nav>
    </div>
  </header>

  <div class="justify-center flex">
    <div>
      <RouterView />
    </div>
  </div>

  <footer>
    <div class="bg-gray-700 pt-8">
      <div class="flex justify-center">
        <div class="grid grid-cols-4 gap-x-4">
          <!-- First column -->
          <div class="flex flex-col">
            <div class="w-48 h-60 p-2">
              <h1 class="text-2xl pb-4">Elektroteknisk forening</h1>
              <h2 class="text-lg">Sparring, faglighed og fællesskab. Vi sætter strøm til elteknik-branchen</h2>
            </div>
          </div>
          <!-- Second column -->
          <div class="flex flex-col">
            <div class="w-48 h-60 p-2">
              <h2 class="text-xl pb-4">Priser</h2>
              <button class="text-lg text-gray-300 hover:text-gray-500 cursor-pointer">Bliv medlem</button>
            </div>
          </div>
          <!-- Third column -->
          <div class="flex flex-col">
            <div class="w-48 h-60 p-2">
              <h2 class="text-xl pb-4">Om foreningen</h2>
              <div>
                <button class="block text-lg text-gray-300 hover:text-gray-500 cursor-pointer pb-2">Nyheder</button>
                <button class="block text-lg text-gray-300 hover:text-gray-500 cursor-pointer pb-2">Om os</button>
                <button class="block text-lg text-gray-300 hover:text-gray-500 cursor-pointer pb-2">Kontakt</button>
                <button class="block text-lg text-gray-300 hover:text-gray-500 cursor-pointer pb-2">Vedtægter</button>
                <button
                  class="block text-lg text-gray-300 hover:text-gray-500 cursor-pointer pb-2">Databeskyttelse</button>
              </div>
            </div>
          </div>
          <!-- Fourth column -->
          <div class="flex flex-col">
            <div class="w-48 h-60 p-2">
              <h2 class="text-xl pb-4">Medlemmer</h2>
              <button
                v-if="!authStore.isAuthenticated"
                @click="openModal"
                class="block text-lg text-gray-300 hover:text-gray-500 cursor-pointer pb-2">
                Log på
              </button>
              <button
                v-else
                @click="handleLogout"
                class="block text-lg text-gray-300 hover:text-gray-500 cursor-pointer pb-2">
                Log ud
              </button>
            </div>
          </div>
        </div>
      </div>
      <div class="flex items-center pt-4 pb-8">
        <img width="100" src="@/assets/dkef-logo.png" alt="dkef logo">
        <h1 class="px-4">| © Elektroteknisk forening 2025</h1>
      </div>
    </div>
  </footer>
</template>

<style scoped></style>
