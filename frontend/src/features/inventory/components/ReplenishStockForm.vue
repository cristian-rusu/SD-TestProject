<script setup lang="ts">
import { ref } from "vue";
defineProps<{ busy: boolean }>();
const emit = defineEmits<{ submit: [quantity: number] }>();
const quantity = ref("");
const error = ref("");
function submit() {
  const n = Number(quantity.value);
  error.value =
    Number.isInteger(n) && n > 0
      ? ""
      : "Enter a whole quantity greater than zero.";
  if (!error.value) emit("submit", n);
}
</script>
<template>
  <form class="stack" novalidate @submit.prevent="!busy && submit()">
    <div>
      <label for="replenish-quantity">Quantity to add</label
      ><input
        id="replenish-quantity"
        v-model="quantity"
        type="number"
        min="1"
        step="1"
        required
        :aria-invalid="!!error"
        aria-describedby="quantity-error"
      />
      <p id="quantity-error" class="field-error">{{ error }}</p>
    </div>
    <div>
      <button :disabled="busy">
        {{ busy ? "Saving…" : "Replenish stock" }}
      </button>
    </div>
  </form>
</template>
