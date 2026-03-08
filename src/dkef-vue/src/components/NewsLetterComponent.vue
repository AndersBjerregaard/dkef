<script setup lang="ts"></script>

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
            <input
              id="name_input"
              name="name_input"
              placeholder="Navn"
              class="rounded-xl bg-theme-soft border border-theme-border p-2 w-full focus:outline-none focus:ring-2 focus:ring-theme-accent focus:border-theme-accent"
              type="text"
            />
          </div>
          <div>
            <input
              id="email_input"
              name="email_input"
              placeholder="Email"
              class="rounded-xl bg-theme-soft border border-theme-border p-2 w-full focus:outline-none focus:ring-2 focus:ring-theme-accent focus:border-theme-accent"
              type="email"
            />
          </div>
          <div class="py-4">
            <input
              class="flex justify-center rounded-xl bg-amber-600 text-navy-950 font-semibold h-12 py-2 px-8 cursor-pointer hover:bg-amber-500 text-lg transition-colors shadow-lg shadow-amber-600/20"
              type="submit"
              value="Tilmeld"
            />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style lang="css" scoped></style>
