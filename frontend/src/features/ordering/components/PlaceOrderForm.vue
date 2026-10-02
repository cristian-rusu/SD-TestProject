<script setup lang="ts">
import { ref } from "vue";
const props = defineProps<{ busy: boolean; productId?: string }>();
const emit = defineEmits<{ submit: [productId: string, quantity: number] }>();
const id = ref(props.productId ?? "");
const quantity = ref("");
const errors = ref<Record<string, string>>({});
function submit() {
  errors.value = {};
  if (
    !/^[\da-f]{8}-[\da-f]{4}-[\da-f]{4}-[\da-f]{4}-[\da-f]{12}$/i.test(
      id.value.trim(),
    )
  )
    errors.value.id = "Enter a valid product ID.";
  const n = Number(quantity.value);
  if (!Number.isInteger(n) || n <= 0)
    errors.value.quantity = "Enter a whole quantity greater than zero.";
  if (!Object.keys(errors.value).length) emit("submit", id.value.trim(), n);
}
</script>
<template>
  <form class="stack" novalidate @submit.prevent="!busy && submit()">
    <div>
      <label for="product-id">Product ID</label
      ><input
        id="product-id"
        v-model="id"
        required
        :aria-invalid="!!errors.id"
        aria-describedby="product-error"
      />
      <p id="product-error" class="field-error">{{ errors.id }}</p>
      <RouterLink v-if="id" :to="`/catalog/${encodeURIComponent(id)}`"
        >View product and current price</RouterLink
      >
    </div>
    <div>
      <label for="order-quantity">Quantity</label
      ><input
        id="order-quantity"
        v-model="quantity"
        type="number"
        min="1"
        step="1"
        required
        :aria-invalid="!!errors.quantity"
        aria-describedby="order-quantity-error"
      />
      <p id="order-quantity-error" class="field-error">{{ errors.quantity }}</p>
    </div>
    <p class="muted">
      The accepted price and order outcome appear after you place the order.
    </p>
    <div>
      <button :disabled="busy">
        {{ busy ? "Placing order…" : "Place order" }}
      </button>
    </div>
  </form>
</template>
