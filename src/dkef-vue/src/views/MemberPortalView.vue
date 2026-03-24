<script setup lang="ts"></script>

<template>
  <main class="py-24">
    <div class="flex justify-center items-center text-center">
      <h1 class="text-4xl">Medlemsportal</h1>
    </div>
    <div class="flex justify-center items-center text-center pt-20">
      <RouterLink to="/change-profile">
        <button
          class="rounded-lg bg-amber-600 h-12 py-2 px-6 cursor-pointer hover:bg-amber-500 active:bg-amber-700 text-navy-950 font-semibold text-base shadow-lg shadow-amber-600/20 transition-all"
        >
          Ret Profil
        </button>
      </RouterLink>
    </div>
  </main>
</template>
