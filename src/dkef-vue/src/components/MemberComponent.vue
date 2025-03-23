<script setup lang="ts">

interface Contact {
  Name: string;
}

const props = withDefaults(defineProps<{ contact?: Contact }>(), {
  contact: () => ({ Name: '' })
});

</script>

<template>
  <div>
    <p>{{ props.contact.Name }}</p>
  </div>
</template>

<style lang="css" scoped></style>
