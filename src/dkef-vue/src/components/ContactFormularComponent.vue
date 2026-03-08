<script setup lang="ts"></script>

<template>
  <div class="flex-row h-96">
    <label for="name_input">Navn</label>
    <br />
    <input
      class="w-full bg-theme-soft border border-theme-border rounded-xl p-2 focus:outline-none focus:ring-2 focus:ring-amber-500 focus:border-amber-500"
      id="name_input"
      name="name_input"
      placeholder="Navn"
      type="text"
    />
    <br />
    <label for="phone_input">Telefon</label>
    <br />
    <input
      class="w-full bg-theme-soft border border-theme-border rounded-xl p-2 focus:outline-none focus:ring-2 focus:ring-amber-500 focus:border-amber-500"
      id="phone_input"
      name="phone_input"
      placeholder="Telefon"
      type="text"
    />
    <br />
    <label for="email_input">Email</label>
    <br />
    <input
      class="w-full bg-theme-soft border border-theme-border rounded-xl p-2 focus:outline-none focus:ring-2 focus:ring-amber-500 focus:border-amber-500"
      id="email_input"
      name="email_input"
      placeholder="Email"
      type="email"
    />
    <br />
    <div>
      <label class="text-xl" for="message_input">Besked</label>
      <br />
      <textarea
        class="w-full h-32 bg-theme-soft border border-theme-border rounded-2xl p-2 focus:outline-none focus:ring-2 focus:ring-amber-500 focus:border-amber-500"
        id="message_input"
        name="message_input"
        placeholder="Skriv din besked her"
      ></textarea>
    </div>
    <br />
    <div>
      <input
        class="flex justify-center rounded-xl bg-amber-600 text-navy-950 font-semibold h-12 px-8 cursor-pointer hover:bg-amber-500 text-lg transition-colors shadow-lg shadow-amber-600/20"
        type="submit"
        value="Send besked"
      />
    </div>
  </div>
</template>

<style lang="css" scoped></style>
