<script setup lang="ts">
import { ref } from "vue";
defineProps<{ label: string; busy?: boolean }>();
const emit = defineEmits<{ lookup: [id: string] }>();
const id = ref("");
</script>
<template>
  <form class="lookup" @submit.prevent="!busy && emit('lookup', id.trim())">
    <label for="lookup-id">{{ label }}</label>
    <div class="inline">
      <input
        id="lookup-id"
        v-model="id"
        required
        placeholder="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
        pattern="[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}"
      /><button :disabled="busy">
        {{ busy ? "Loading…" : "Find record" }}
      </button>
    </div>
  </form>
</template>
