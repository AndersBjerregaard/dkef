<script setup lang="ts"></script>

<template>
  <div class="flex-row h-96">
    <label for="name_input">Navn</label>
    <br>
    <input class="w-full bg-gray-800 border-0 rounded-xl p-2" id="name_input" name="name_input" placeholder="Navn"
      type="text">
    <br>
    <label for="phone_input">Telefon</label>
    <br>
    <input class="w-full bg-gray-800 border-0 rounded-xl p-2" id="phone_input" name="phone_input" placeholder="Telefon"
      type="text">
    <br>
    <label for="email_input">Email</label>
    <br>
    <input class="w-full bg-gray-800 border-0 rounded-xl p-2" id="email_input" name="email_input" placeholder="Email"
      type="email">
    <br>
    <div>
      <label class="text-xl" for="message_input">Besked</label>
      <br>
      <textarea class="w-full h-32 bg-gray-800 border-0 rounded-2xl p-2" id="message_input" name="message_input"
        placeholder="Skriv din besked her"></textarea>
    </div>
    <br>
    <div>
      <input class="flex justify-center rounded bg-gray-600 h-12 px-8 cursor-pointer hover:bg-gray-800 text-lg"
        type="submit" value="Send besked">
    </div>
  </div>
</template>

<style lang="css" scoped></style>
