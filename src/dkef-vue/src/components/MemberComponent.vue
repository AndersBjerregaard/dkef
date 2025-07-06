<script setup lang="ts">
import { type Contact } from '@/types/members'
import { computed, ref } from 'vue'
import BaseModal from '@/components/BaseModal.vue';

// Modal state
const isOpen = ref(false);
const isLoading = ref(false);

function openModal() {
  isOpen.value = true;
}

function closeModal() {
  isOpen.value = false;
}

const props = defineProps<{ contact: Contact, index: number }>();

const fields = computed(() => [
  props.contact?.firstName,
  props.contact?.email,
  props.contact?.privatePhone,
  props.contact?.primarySection,
  props.contact?.privateAddress
]);

</script>

<template>
  <div class="border-x-2 border-gray-900 w-full justify-between flex hover:bg-gray-200 hover:text-gray-900 cursor-pointer"
    @click="openModal"
    :class="{ 'bg-black': index % 2 == 0, 'bg-gray-800': index % 2 == 1 }"
  >
    <div v-for="(field, i) in fields" :key="i"
      class="border-2 border-gray-900 h-10 py-1 px-1 flex-1 min-w-0 flex justify-center"
    >
      <span class="truncate">{{ field }}</span>
    </div>
  </div>
  <BaseModal
    :is-open="isOpen"
    :title="'Medlem'"
    :is-loading="isLoading"
    @close="closeModal"
  >
    <h2 class="text-2xl">Ola</h2>
  </BaseModal>
</template>

<style lang="css" scoped></style>
