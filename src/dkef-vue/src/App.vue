<script setup lang="ts">
import { RouterView } from 'vue-router'
// import HelloWorld from './components/HelloWorld.vue'
</script>

<template>
  <header>
    <div>
      <!-- HelloWorld msg="You did it!" /> -->

      <nav class="flex bg-gray-700 p-4">
        <img width="100" height="100" src="@/assets/dkef-logo.png" alt="DKEF logo">
        <div class="flex w-full justify-between p-4 items-center">
          <button class="rounded bg-gray-600 h-12 p-2">Medlemsfordele</button>
          <button class="rounded bg-gray-600 h-12 p-2">Arrangementer og nyheder</button>
          <button class="rounded bg-gray-600 h-12 p-2">Om foreningen</button>
          <button class="rounded bg-gray-600 h-12 p-2">Kontakt os</button>
          <button class="rounded bg-gray-600 h-12 p-2">Log på</button>
        </div>
        <!-- <RouterLink to="/">Home</RouterLink>
        <RouterLink to="/about">About</RouterLink> -->
      </nav>
    </div>
  </header>

  <RouterView />
</template>

<style scoped>
</style>
