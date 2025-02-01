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
          <img width="100" height="100" src="@/assets/dkef-logo.png" alt="DKEF logo">
        </RouterLink>
        <div class="w-full">
          <div class="flex p-4 justify-end items-center">
            <div class="p-3">
              <button class="rounded bg-gray-600 h-12 p-2 cursor-pointer hover:bg-gray-800">Medlemsfordele</button>
            </div>
            <div class="p-3">
              <button class="rounded bg-gray-600 h-12 p-2 cursor-pointer hover:bg-gray-800">Arrangementer og
                nyheder</button>
            </div>
            <div class="p-3">
              <button class="rounded bg-gray-600 h-12 p-2 cursor-pointer hover:bg-gray-800">Om foreningen</button>
            </div>
            <div class="p-3">
              <button class="rounded bg-gray-600 h-12 p-2 cursor-pointer hover:bg-gray-800">Kontakt os</button>
            </div>
            <div class="p-3">
              <button class="rounded bg-gray-600 h-12 p-2 cursor-pointer hover:bg-gray-800">Log på</button>
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
</template>

<style scoped></style>
