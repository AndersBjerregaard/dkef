<script setup lang="ts">
import type { Contact } from '@/types/members'

defineProps<{ member: Contact }>()
</script>

<template>
  <div
    class="bg-theme-soft border-2 border-theme-border rounded-lg p-4 hover:bg-theme-border hover:text-theme-accent transition-colors"
  >
    <h3 class="font-semibold text-lg">{{ member.name }}</h3>
    <p v-if="member.companyName">{{ member.companyName }}</p>
    <div class="mt-3 space-y-1 text-sm">
      <p v-if="member.email">
        <span class="font-medium">Email: </span>
        <a :href="`mailto:${member.email}`" class="text-blue-500 hover:underline">{{
          member.email
        }}</a>
      </p>
      <p v-if="member.privatePhoneNumber">
        <span class="font-medium">Telefon: </span>
        <a :href="`tel:${member.privatePhoneNumber}`" class="text-blue-500 hover:underline">{{
          member.privatePhoneNumber
        }}</a>
      </p>
    </div>
  </div>
</template>

<style lang="css" scoped></style>
