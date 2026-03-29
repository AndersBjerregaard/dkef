<script setup lang="ts">
import { RouterView, RouterLink, useRouter } from 'vue-router'
import {
  Menu,
  MenuButton,
  MenuItems,
  MenuItem,
  TransitionRoot,
  TransitionChild,
  Dialog,
  DialogPanel,
  DialogTitle,
} from '@headlessui/vue'
import { ref, computed } from 'vue'
import { useAuthStore } from '@/stores/authStore'
import { useThemeStore } from '@/stores/themeStore'

const authStore = useAuthStore()
const themeStore = useThemeStore()
const isOpen = ref(false)
const email = ref('')
const password = ref('')
const loginError = ref('')
const isLoggingIn = ref(false)
const router = useRouter()

const firstName = computed(() => {
  if (!authStore) {
    return ''
  }
  if (!authStore.user) {
    return ''
  }
  // Extract first name from full name
  const fullName = authStore.user.name
  return fullName.split(' ')[0]
})

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
      password: password.value,
    })
    closeModal()
  } catch (error: unknown) {
    const errorMessage =
      error && typeof error === 'object' && 'response' in error
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

async function goto(view: string) {
  await router.push({ name: view })
  window.scrollTo({ top: 0, behavior: 'smooth' })
}
</script>

<template>
  <header>
    <div>
      <nav class="flex bg-theme-soft border-b border-amber-500/20 p-1 sm:p-4">
        <RouterLink to="/">
          <div class="w-10 sm:w-20">
            <img src="@/assets/dkef-logo.png" alt="DKEF logo" />
          </div>
        </RouterLink>

        <!-- Inline buttons (visible on larger screens ) -->
        <div class="hidden lg:flex lg:w-full">
          <div class="flex p-4 w-full justify-end items-center gap-1">
            <div class="p-1" v-if="authStore.isAdmin">
              <RouterLink to="/members">
                <button
                  class="rounded-lg bg-theme-mute h-10 px-4 cursor-pointer text-theme-text hover:bg-theme-mute hover:text-theme-accent transition-colors text-sm font-medium"
                >
                  Medlemmer
                </button>
              </RouterLink>
            </div>
            <div class="p-1">
              <RouterLink to="/advantages">
                <button
                  class="rounded-lg bg-theme-mute h-10 px-4 cursor-pointer text-theme-text hover:bg-theme-mute hover:text-theme-accent transition-colors text-sm font-medium"
                >
                  Medlemsfordele
                </button>
              </RouterLink>
            </div>
            <div class="p-1">
              <RouterLink to="/events-and-news">
                <button
                  class="rounded-lg bg-theme-mute h-10 px-4 cursor-pointer text-theme-text hover:bg-theme-mute hover:text-theme-accent transition-colors text-sm font-medium"
                >
                  Arrangementer og nyheder
                </button>
              </RouterLink>
            </div>
            <div class="p-1">
              <Menu as="div" class="relative inline-block text-left" v-slot="{ open }">
                <div>
                  <MenuButton
                    class="rounded-lg bg-theme-mute h-10 px-4 cursor-pointer text-theme-text hover:bg-theme-mute hover:text-theme-accent transition-colors text-sm font-medium flex items-center gap-2"
                  >
                    Om foreningen
                    <svg
                      :class="['w-4 h-4 transition-transform duration-200', { 'rotate-180': open }]"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                    >
                      <path
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        stroke-width="2"
                        d="M19 14l-7 7m0 0l-7-7m7 7V3"
                      />
                    </svg>
                  </MenuButton>
                </div>
                <transition
                  enter-active-class="transition ease-out duration-100"
                  enter-from-class="transform opacity-0 scale-95"
                  enter-to-class="transform opacity-100 scale-100"
                  leave-active-class="transition ease-in duration-75"
                  leave-from-class="transform opacity-100 scale-100"
                  leave-to-class="transform opacity-0 scale-95"
                >
                  <MenuItems
                    class="absolute left-0 z-10 mt-2 w-56 origin-top-left rounded-lg bg-theme-bg shadow-lg ring-1 ring-theme-border focus:outline-none"
                  >
                    <div class="px-1 py-1">
                      <MenuItem v-slot="{ active, close }">
                        <RouterLink to="/about">
                          <button
                            :class="[
                              active ? 'bg-theme-border text-theme-accent' : 'text-theme-text',
                              'group flex w-full items-center rounded-lg px-2 py-2 text-sm cursor-pointer transition-colors',
                            ]"
                            @click="close"
                          >
                            Om foreningen
                          </button>
                        </RouterLink>
                      </MenuItem>
                      <MenuItem v-slot="{ active, close }">
                        <RouterLink to="/the-boards">
                          <button
                            :class="[
                              active ? 'bg-theme-border text-theme-accent' : 'text-theme-text',
                              'group flex w-full items-center rounded-lg px-2 py-2 text-sm cursor-pointer transition-colors',
                            ]"
                            @click="close"
                          >
                            Bestyrelserne
                          </button>
                        </RouterLink>
                      </MenuItem>
                    </div>
                  </MenuItems>
                </transition>
              </Menu>
            </div>
            <div class="p-1">
              <RouterLink to="/contact">
                <button
                  class="rounded-lg bg-theme-mute h-10 px-4 cursor-pointer text-theme-text hover:bg-theme-mute hover:text-theme-accent transition-colors text-sm font-medium"
                >
                  Kontakt os
                </button>
              </RouterLink>
            </div>
            <div class="p-1" v-if="!authStore.isAuthenticated">
              <button
                class="rounded-lg bg-amber-600 h-10 px-4 cursor-pointer text-navy-950 font-semibold hover:bg-amber-500 active:bg-amber-700 transition-colors text-sm shadow-lg shadow-amber-600/20"
                @click="openModal"
              >
                Log på
              </button>
            </div>
            <div class="p-1 flex items-center gap-3" v-else>
              <span class="text-theme-accent font-medium text-sm">Hej, {{ firstName }}</span>
              <button
                class="rounded-lg bg-theme-mute h-10 px-4 cursor-pointer text-theme-text hover:bg-theme-mute hover:text-theme-accent transition-colors text-sm font-medium"
                @click="handleLogout"
              >
                Log ud
              </button>
            </div>
            <div class="p-1" v-if="!authStore.isAuthenticated">
              <RouterLink to="/register">
                <button
                  class="rounded-lg border border-amber-500/40 h-10 px-4 cursor-pointer text-theme-accent hover:bg-amber-500/10 transition-colors text-sm font-medium"
                >
                  Nyt medlem?
                </button>
              </RouterLink>
            </div>
            <!-- Member Portal -->
            <div class="p-1" v-if="authStore.isAuthenticated">
              <RouterLink to="/member-portal">
                <button
                  class="rounded-lg bg-theme-mute h-10 px-4 cursor-pointer text-theme-text hover:bg-theme-mute hover:text-theme-accent transition-colors text-sm font-medium"
                >
                  Medlemsportal
                </button>
              </RouterLink>
            </div>
            <!-- Theme toggle -->
            <div class="p-1">
              <button
                class="rounded-lg bg-theme-mute h-10 w-10 cursor-pointer text-theme-text hover:text-theme-accent transition-colors text-lg flex items-center justify-center"
                @click="themeStore.toggleTheme()"
                :title="themeStore.isDark() ? 'Skift til lyst tema' : 'Skift til mørkt tema'"
              >
                {{ themeStore.isDark() ? '☀️' : '🌙' }}
              </button>
            </div>
          </div>
        </div>
        <TransitionRoot appear :show="isOpen" as="template">
          <Dialog as="div" @close="closeModal" class="relative z-10">
            <TransitionChild
              as="template"
              enter="duration-300 ease-out"
              enter-from="opacity-0"
              enter-to="opacity-100"
              leave="duration-200 ease-in"
              leave-from="opacity-100"
              leave-to="opacity-0"
            >
              <div class="fixed inset-0 bg-black/50"></div>
            </TransitionChild>

            <div class="fixed inset-0 overflow-y-auto">
              <div class="flex min-h-full items-center justify-center p-4 text-center">
                <TransitionChild
                  as="template"
                  enter="duration-300 ease-out"
                  enter-from="opacity-0 scale-95"
                  enter-to="opacity-100 scale-100"
                  leave="duration-200 ease-in"
                  leave-from="opacity-100 scale-100"
                  leave-to="opacity-0 scale-95"
                >
                  <DialogPanel
                    class="w-full max-w-md transform overflow-hidden rounded-2xl bg-theme-mute border border-theme-border p-6 text-left align-middle shadow-2xl transition-all"
                  >
                    <DialogTitle as="h3" class="text-lg leading-6 text-theme-heading">
                      Log på
                    </DialogTitle>
                    <form @submit.prevent="handleLogin" class="mt-4">
                      <div class="mb-4">
                        <label for="email" class="block text-sm font-medium text-theme-text mb-2">
                          Email
                        </label>
                        <input
                          id="email"
                          v-model="email"
                          type="email"
                          required
                          autocomplete="username"
                          class="w-full px-3 py-2 bg-theme-soft border border-theme-border rounded-lg shadow-sm focus:outline-none focus:ring-2 focus:ring-theme-accent focus:border-theme-accent text-theme-heading placeholder-slate-500"
                          placeholder="din@email.dk"
                        />
                      </div>
                      <div class="mb-4">
                        <label
                          for="password"
                          class="block text-sm font-medium text-theme-text mb-2"
                        >
                          Adgangskode
                        </label>
                        <input
                          id="password"
                          v-model="password"
                          type="password"
                          required
                          autocomplete="current-password"
                          class="w-full px-3 py-2 bg-theme-soft border border-theme-border rounded-lg shadow-sm focus:outline-none focus:ring-2 focus:ring-theme-accent focus:border-theme-accent text-theme-heading placeholder-slate-500"
                          placeholder="••••••••"
                        />
                      </div>

                      <div class="mb-4 text-right">
                        <RouterLink
                          to="/forgot-password"
                          @click="closeModal"
                          class="text-sm text-theme-accent hover:text-amber-300 cursor-pointer"
                        >
                          Glemt adgangskode?
                        </RouterLink>
                      </div>

                      <div
                        v-if="loginError"
                        class="mb-4 p-3 bg-red-900/50 border border-red-700 text-red-300 rounded-lg"
                      >
                        {{ loginError }}
                      </div>

                      <div class="flex gap-3 py-2">
                        <button
                          type="submit"
                          :disabled="isLoggingIn"
                          class="inline-flex justify-center rounded-lg bg-amber-600 px-4 py-2 text-sm font-semibold text-navy-950 hover:bg-amber-500 focus:outline-none focus-visible:ring-2 focus-visible:ring-theme-accent focus-visible:ring-offset-2 focus-visible:ring-offset-theme-mute cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed transition-colors shadow-lg shadow-amber-600/20"
                        >
                          {{ isLoggingIn ? 'Logger på...' : 'Log på' }}
                        </button>
                        <button
                          type="button"
                          class="inline-flex justify-center rounded-lg border border-theme-border bg-theme-soft px-4 py-2 text-sm font-medium text-theme-text hover:bg-theme-mute focus:outline-none focus-visible:ring-2 focus-visible:ring-theme-accent focus-visible:ring-offset-2 focus-visible:ring-offset-theme-mute cursor-pointer transition-colors"
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
          <div class="w-24 sm:w-40 flex justify-between">
            <!-- Theme toggle (mobile) -->
            <button
              class="rounded-lg sm:text-2xl bg-theme-mute border border-theme-border h-8 sm:h-12 w-10 sm:w-12 cursor-pointer hover:text-theme-accent transition-colors text-theme-text inline-flex items-center justify-center mr-3 sm:mr-4"
              @click="themeStore.toggleTheme()"
              :title="themeStore.isDark() ? 'Skift til lyst tema' : 'Skift til mørkt tema'"
            >
              {{ themeStore.isDark() ? '☀️' : '🌙' }}
            </button>
            <div class="relative">
              <Menu>
                <MenuButton
                  class="text-lg sm:text-2xl rounded-lg bg-theme-mute border border-theme-border h-8 sm:h-12 w-10 sm:w-20 sm:p-2 cursor-pointer hover:bg-theme-mute hover:text-theme-accent transition-colors text-theme-text"
                >
                  ≡</MenuButton
                >
                <transition
                  enter-active-class="transition duration-100 ease-out"
                  enter-from-class="transform scale-95 opacity-0"
                  enter-to-class="transform scale-100 opacity-100"
                  leave-active-class="transition duration-75 ease-in"
                  leave-from-class="transform scale-100 opacity-100"
                  leave-to-class="transform scale-95 opacity-0"
                >
                  <MenuItems
                    class="absolute right-0 top-full mt-2 w-56 origin-top-right divide-y divide-theme-border rounded-xl bg-theme-mute border border-theme-border shadow-2xl ring-1 ring-black/20 focus:outline-none z-50"
                  >
                    <div class="px-1 py-1">
                      <MenuItem v-if="authStore.isAdmin" v-slot="{ active, close }">
                        <RouterLink to="/members">
                          <button
                            :class="[
                              active ? 'bg-theme-border text-theme-accent' : 'text-theme-text',
                              'group flex w-full items-center rounded-lg px-2 py-2 text-sm cursor-pointer transition-colors',
                            ]"
                            @click="close"
                          >
                            Medlemmer
                          </button>
                        </RouterLink>
                      </MenuItem>
                      <MenuItem v-slot="{ active, close }">
                        <RouterLink to="/advantages">
                          <button
                            :class="[
                              active ? 'bg-theme-border text-theme-accent' : 'text-theme-text',
                              'group flex w-full items-center rounded-lg px-2 py-2 text-sm cursor-pointer transition-colors',
                            ]"
                            @click="close"
                          >
                            Medlemsfordele
                          </button>
                        </RouterLink>
                      </MenuItem>
                      <MenuItem v-slot="{ active, close }">
                        <RouterLink to="/events-and-news">
                          <button
                            :class="[
                              active ? 'bg-theme-border text-theme-accent' : 'text-theme-text',
                              'group flex w-full items-center rounded-lg px-2 py-2 text-sm cursor-pointer transition-colors',
                            ]"
                            @click="close"
                          >
                            Arrangementer og nyheder
                          </button>
                        </RouterLink>
                      </MenuItem>
                      <MenuItem v-slot="{ active, close }">
                        <RouterLink to="/about">
                          <button
                            :class="[
                              active ? 'bg-theme-border text-theme-accent' : 'text-theme-text',
                              'group flex w-full items-center rounded-lg px-2 py-2 text-sm cursor-pointer transition-colors',
                            ]"
                            @click="close"
                          >
                            Om foreningen
                          </button>
                        </RouterLink>
                      </MenuItem>
                      <MenuItem v-slot="{ active, close }">
                        <RouterLink to="/the-boards">
                          <button
                            :class="[
                              active ? 'bg-theme-border text-theme-accent' : 'text-theme-text',
                              'group flex w-full items-center rounded-lg px-2 py-2 text-sm cursor-pointer transition-colors',
                            ]"
                            @click="close"
                          >
                            Bestyrelserne
                          </button>
                        </RouterLink>
                      </MenuItem>
                      <MenuItem v-slot="{ active, close }">
                        <RouterLink to="/contact">
                          <button
                            :class="[
                              active ? 'bg-theme-border text-theme-accent' : 'text-theme-text',
                              'group flex w-full items-center rounded-lg px-2 py-2 text-sm cursor-pointer transition-colors',
                            ]"
                            @click="close"
                          >
                            Kontakt os
                          </button>
                        </RouterLink>
                      </MenuItem>
                      <MenuItem v-if="authStore.isAuthenticated" v-slot="{ active, close }">
                        <RouterLink to="/member-portal">
                          <button
                            :class="[
                              active ? 'bg-theme-border text-theme-accent' : 'text-theme-text',
                              'group flex w-full items-center rounded-lg px-2 py-2 text-sm cursor-pointer transition-colors',
                            ]"
                            @click="close"
                          >
                            Medlemsportal
                          </button>
                        </RouterLink>
                      </MenuItem>
                      <MenuItem v-if="!authStore.isAuthenticated" v-slot="{ active }">
                        <button
                          :class="[
                            active ? 'bg-amber-500/20 text-theme-accent' : 'text-theme-accent',
                            'group flex w-full items-center rounded-lg px-2 py-2 text-sm cursor-pointer font-semibold transition-colors',
                          ]"
                          @click="openModal"
                        >
                          Log på
                        </button>
                      </MenuItem>
                      <MenuItem v-else v-slot="{ active }">
                        <button
                          :class="[
                            active ? 'bg-theme-border text-theme-accent' : 'text-theme-text',
                            'group flex w-full items-center rounded-lg px-2 py-2 text-sm cursor-pointer transition-colors',
                          ]"
                          @click="handleLogout"
                        >
                          Log ud
                        </button>
                      </MenuItem>
                    </div>
                  </MenuItems>
                </transition>
              </Menu>
            </div>
          </div>
        </div>
      </nav>
    </div>
  </header>

  <div>
    <RouterView />
  </div>

  <footer>
    <div class="bg-theme-soft border-t border-amber-500/20 pt-8">
      <div class="flex justify-center">
        <div class="grid grid-cols-4 gap-x-4">
          <!-- First column -->
          <div class="flex flex-col">
            <div class="w-48 h-60 p-2">
              <h1 class="text-2xl pb-4 text-theme-heading">Elektroteknisk forening</h1>
              <p class="text-theme-text text-sm leading-relaxed">
                Sparring, faglighed og fællesskab. Vi sætter strøm til elteknik-branchen
              </p>
            </div>
          </div>
          <!-- Second column -->
          <div class="flex flex-col">
            <div class="w-48 h-60 p-2">
              <h2 class="text-xl pb-4 text-theme-accent">Priser</h2>
              <button
                class="text-sm text-theme-text hover:text-theme-accent cursor-pointer transition-colors"
                @click="goto('register')"
              >
                Bliv medlem
              </button>
            </div>
          </div>
          <!-- Third column -->
          <div class="flex flex-col">
            <div class="w-48 h-60 p-2">
              <h2 class="text-xl pb-4 text-theme-accent">Om foreningen</h2>
              <div>
                <button
                  class="block text-sm text-theme-text hover:text-theme-accent cursor-pointer pb-2 transition-colors"
                  @click="goto('events-and-news')"
                >
                  Nyheder
                </button>
                <button
                  class="block text-sm text-theme-text hover:text-theme-accent cursor-pointer pb-2 transition-colors"
                  @click="goto('about')"
                >
                  Om os
                </button>
                <button
                  class="block text-sm text-theme-text hover:text-theme-accent cursor-pointer pb-2 transition-colors"
                  @click="goto('contact')"
                >
                  Kontakt
                </button>
                <button
                  class="block text-sm text-theme-text hover:text-theme-accent cursor-pointer pb-2 transition-colors"
                  @click="goto('statutes')"
                >
                  Vedtægter
                </button>
                <button
                  class="block text-sm text-theme-text hover:text-theme-accent cursor-pointer pb-2 transition-colors"
                  @click="goto('privacy-policy')"
                >
                  Databeskyttelse
                </button>
              </div>
            </div>
          </div>
          <!-- Fourth column -->
          <div class="flex flex-col">
            <div class="w-48 h-60 p-2">
              <h2 class="text-xl pb-4 text-theme-accent">Medlemmer</h2>
              <button
                v-if="!authStore.isAuthenticated"
                @click="openModal"
                class="block text-sm text-theme-text hover:text-theme-accent cursor-pointer pb-2 transition-colors"
              >
                Log på
              </button>
              <button
                v-else
                @click="handleLogout"
                class="block text-sm text-theme-text hover:text-theme-accent cursor-pointer pb-2 transition-colors"
              >
                Log ud
              </button>
            </div>
          </div>
        </div>
      </div>
      <div class="flex items-center pt-4 pb-6 px-8 border-t border-theme-border mt-4">
        <img width="80" src="@/assets/dkef-logo.png" alt="dkef logo" />
        <p class="px-4 text-theme-muted text-sm">| © Elektroteknisk forening 2025</p>
      </div>
    </div>
  </footer>
</template>

<style scoped></style>
