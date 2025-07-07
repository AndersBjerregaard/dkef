<script setup lang="ts">
import { RouterView, RouterLink } from 'vue-router'
import {
  Menu, MenuButton, MenuItems, MenuItem,
  TransitionRoot, TransitionChild,
  Dialog, DialogPanel, DialogTitle
} from '@headlessui/vue'
import { ref } from 'vue'

const isOpen = ref(false)

function closeModal() {
  isOpen.value = false
}

function openModal() {
  isOpen.value = true
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
            <div class="p-3">
              <RouterLink to="/members">
                <button class="rounded bg-gray-600 h-12 w-36 p-2 cursor-pointer hover:bg-gray-800">Medlemmer
                  (auth)</button>
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
            <div class="p-3">
              <button class="rounded bg-gray-600 h-12 w-20 p-2 cursor-pointer hover:bg-gray-800" @click="openModal">
                Log på
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
                      Login
                    </DialogTitle>
                    <div class="mt-2">
                      <p class="text-sm text-gray-500">
                        Du er nu logget på.
                      </p>
                    </div>

                    <div class="mt-4">
                      <button type="button"
                        class="inline-flex justify-center rounded-md border border-transparent bg-blue-100 px-4 py-2 text-sm font-medium text-blue-900 hover:bg-blue-200 focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 focus-visible:ring-offset-2 cursor-pointer"
                        @click="closeModal">
                        Fortsæt til DKEF
                      </button>
                    </div>
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
                    <MenuItem v-slot="{ active, close }">
                    <RouterLink to="/members">
                      <button
                        :class="[active ? 'bg-gray-800' : 'bg-gray-600', 'group flex w-full items-center rounded-md px-2 py-2 text-sm cursor-pointer']"
                        @click="close">
                        Medlemmer (auth)
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
                    <MenuItem v-slot="{ active }">
                    <button
                      :class="[active ? 'bg-gray-800' : 'bg-gray-600', 'group flex w-full items-center rounded-md px-2 py-2 text-sm cursor-pointer']">
                      Kontakt os
                    </button>
                    </MenuItem>
                    <MenuItem v-slot="{ active }">
                    <button
                      :class="[active ? 'bg-gray-800' : 'bg-gray-600', 'group flex w-full items-center rounded-md px-2 py-2 text-sm cursor-pointer']">
                      Log på
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
              <button class="block text-lg text-gray-300 hover:text-gray-500 cursor-pointer pb-2">Log på</button>
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
