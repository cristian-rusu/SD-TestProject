<script setup lang="ts">
import { ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import PlaceOrderForm from "../components/PlaceOrderForm.vue";
import { placeOrder } from "../api/orderingApi";
import { errorMessage } from "../../../shared/api/apiError";
import ErrorMessage from "../../../shared/components/ErrorMessage.vue";
const id = String(useRoute().query.productId ?? "");
const router = useRouter();
const busy = ref(false);
const error = ref("");
async function submit(productId: string, quantity: number) {
  if (busy.value) return;
  busy.value = true;
  error.value = "";
  try {
    const result = await placeOrder(productId, quantity);
    await router.push(`/orders/${result.orderId}`);
  } catch (e) {
    error.value = errorMessage(e);
  } finally {
    busy.value = false;
  }
}
</script>
<template>
  <RouterLink to="/orders" class="back">← Orders</RouterLink>
  <div class="page-heading">
    <div>
      <p class="eyebrow">Ordering</p>
      <h1>Place order</h1>
    </div>
  </div>
  <section class="card form-card">
    <ErrorMessage :message="error" /><PlaceOrderForm
      :busy="busy"
      :product-id="id"
      @submit="submit"
    />
  </section>
</template>
