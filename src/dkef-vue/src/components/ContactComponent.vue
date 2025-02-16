<script setup lang="ts">
import ContactFormularComponent from './ContactFormularComponent.vue';
import NewsLetterComponent from './NewsLetterComponent.vue';

</script>

<template>
  <!-- Contact -->
  <div class="flex justify-center items-center gap-x-4 px-4 pt-8 pb-32">
    <div class="flex-row w-[45%]">
      <h1 class="text-4xl py-8">Kontakt os</h1>
      <p class="text-lg">
        Vi er klar til at hjælpe dig. Skriv din henvendelse på formularen. Vi besvarer din henvendelse hurtigst muligt.
      </p>
    </div>
    <div class="w-[45%]">
      <ContactFormularComponent />
    </div>
  </div>

  <!-- Contact info -->
  <div class="pb-20 flex">
    <div class="flex w-full justify-between items-center text-center px-8 bg-gray-800 h-40">
      <!-- Email -->
      <div class="flex gap-x-4">
        <div class="w-20 h-20 border-0 rounded-full bg-gray-400 flex justify-center items-center">
          <div class="w-12 h-12">
            <img class="w-full h-full object-fill" src="@/assets/envelope-icon.svg">
          </div>
        </div>
        <div class="flex-row">
          <h1 class="text-xl">Email</h1>
          <h3 class="text-lg">info@dkef.dk</h3>
        </div>
      </div>
      <!-- Phone -->
      <div class="flex gap-x-4">
        <div class="w-20 h-20 border-0 rounded-full bg-gray-400 flex justify-center items-center">
          <div class="w-12 h-12">
            <img class="w-full h-full object-fill" src="@/assets/call-square-icon.svg">
          </div>
        </div>
        <div class="flex-row">
          <h1 class="text-xl">Telefon</h1>
          <h3 class="text-lg">+45 42 57 70 69</h3>
        </div>
      </div>
      <!-- Address -->
      <div class="flex gap-x-4">
        <div class="w-20 h-20 border-0 rounded-full bg-gray-400 flex justify-center items-center">
          <div class="w-12 h-12">
            <img class="w-full h-full object-fill" src="@/assets/maps-pin-black-icon.svg">
          </div>
        </div>
        <div class="flex-row">
          <h1 class="text-xl">Adresse</h1>
          <h3 class="text-lg">Elleholmvej 28C</h3>
          <h3 class="text-lg">5250 Odense SV</h3>
        </div>
      </div>
    </div>
  </div>

  <NewsLetterComponent />
</template>

<style lang="css" scoped></style>
