<script setup lang="ts">
import { RouterView, RouterLink } from 'vue-router'
// import HelloWorld from './components/HelloWorld.vue'
</script>

<template>
  <header>
    <div>
      <!-- HelloWorld msg="You did it!" /> -->

      <nav class="flex bg-gray-700 p-4 w-screen">
        <RouterLink to="/">
          <div class="w-24">
            <img width="100" height="100" src="@/assets/dkef-logo.png" alt="DKEF logo">
          </div>
        </RouterLink>
        <div class="w-full">
          <div class="flex p-4 justify-end items-center">
            <div class="p-3">
              <button class="rounded bg-gray-600 h-12 w-36 p-2 cursor-pointer hover:bg-gray-800">Medlemsfordele</button>
            </div>
            <div class="p-3">
              <button class="rounded bg-gray-600 h-12 w-52 p-2 cursor-pointer hover:bg-gray-800">Arrangementer og
                nyheder</button>
            </div>
            <div class="p-3">
              <button class="rounded bg-gray-600 h-12 w-32 p-2 cursor-pointer hover:bg-gray-800">Om foreningen</button>
            </div>
            <div class="p-3">
              <button class="rounded bg-gray-600 h-12 w-24 p-2 cursor-pointer hover:bg-gray-800">Kontakt os</button>
            </div>
            <div class="p-3">
              <button class="rounded bg-gray-600 h-12 w-20 p-2 cursor-pointer hover:bg-gray-800">Log på</button>
            </div>
          </div>
        </div>
        <!-- <RouterLink to="/">Home</RouterLink>
        <RouterLink to="/about">About</RouterLink> -->
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
