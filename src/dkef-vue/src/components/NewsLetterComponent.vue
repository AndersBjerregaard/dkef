<script setup lang="ts">
</script>

<template>
  <div class="pb-16">
    <div class="grid grid-cols-2 justify-center gap-x-8">
      <!-- Left Column -->
      <div class="flex flex-col gap-4 items-end">
        <div class="w-3/6 h-54">
          <h1 class="text-4xl">Vil du have besked som én af de første når der sker noget nyt?</h1>
        </div>
      </div>

      <!-- Right Column -->
      <div class="flex flex-col gap-4 items-start">
        <div class="w-3/6 h-54">
          <div class="py-4">
            <input id="name_input" name="name_input" placeholder="Navn" class="rounded bg-gray-800 p-2 w-full"
              type="text">
          </div>
          <div>
            <input id="email_input" name="email_input" placeholder="Email" class="rounded bg-gray-800 p-2 w-full"
              type="email">
          </div>
          <div class="py-4">
            <input
              class="flex justify-center rounded bg-gray-600 h-12 py-2 px-8 cursor-pointer hover:bg-gray-800 text-lg"
              type="submit" value="Timeld">
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style lang="css" scoped></style>
