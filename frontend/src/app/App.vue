<script setup lang="ts">
import AppLayout from "./layouts/AppLayout.vue";
</script>
<template>
  <AppLayout><RouterView :key="$route.fullPath" /></AppLayout>
</template>
