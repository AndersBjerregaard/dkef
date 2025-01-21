<script setup lang="ts">
import { RouterView } from 'vue-router'
// import HelloWorld from './components/HelloWorld.vue'
</script>

<template>
  <header>
    <div>
      <!-- HelloWorld msg="You did it!" /> -->

      <nav class="flex bg-gray-700 justify-between p-4">
        <img width="125" height="125" alt="DKEF logo">
        <button>Medlemsfordele</button>
        <button>Arrangementer og nyheder</button>
        <button>Om foreningen</button>
        <button>Kontakt os</button>
        <button>Log på</button>
        <!-- <RouterLink to="/">Home</RouterLink>
        <RouterLink to="/about">About</RouterLink> -->
      </nav>
    </div>
  </header>

  <RouterView />
</template>

<style scoped></style>
