<script setup lang="ts">
defineProps<{ status: string }>();
</script>
<template>
  <span
    class="badge"
    :class="status.toLowerCase()"
    :aria-label="`Status: ${status}`"
    >{{ status }}</span
  >
</template>
