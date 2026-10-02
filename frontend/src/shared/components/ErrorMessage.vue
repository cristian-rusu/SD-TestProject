<script setup lang="ts">
defineProps<{ message: string }>();
</script>
<template>
  <div v-if="message" class="error-message" role="alert">{{ message }}</div>
</template>
