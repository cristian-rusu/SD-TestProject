<script setup lang="ts">
defineProps<{ title: string }>();
</script>
<template>
  <div class="empty-state">
    <h2>{{ title }}</h2>
    <p class="muted"><slot /></p>
  </div>
</template>
