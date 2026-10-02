<script setup lang="ts">
import { onMounted, onUnmounted, ref } from "vue";
import { useRoute } from "vue-router";
import { getOrder } from "../api/orderingApi";
import type { OrderDto } from "../types/ordering";
import { errorMessage } from "../../../shared/api/apiError";
import ErrorMessage from "../../../shared/components/ErrorMessage.vue";
import LoadingState from "../../../shared/components/LoadingState.vue";
import OrderSummary from "../components/OrderSummary.vue";
const id = String(useRoute().params.orderId);
const order = ref<OrderDto>();
const busy = ref(false);
const error = ref("");
const timedOut = ref(false);
let timer: ReturnType<typeof setTimeout> | undefined;
let disposed = false;
let attempts = 0;
async function refresh(manual = false) {
  if (busy.value || disposed) return;
  clearTimeout(timer);
  if (manual) {
    attempts = 0;
    timedOut.value = false;
  }
  busy.value = true;
  error.value = "";
  try {
    const result = await getOrder(id);
    if (disposed) return;
    order.value = result;
    if (result.status === "Pending") {
      if (++attempts < 30) timer = setTimeout(() => refresh(), 1000);
      else timedOut.value = true;
    }
  } catch (e) {
    if (!disposed) error.value = errorMessage(e);
  } finally {
    busy.value = false;
  }
}
onMounted(() => refresh());
onUnmounted(() => {
  disposed = true;
  clearTimeout(timer);
});
</script>
<template>
  <RouterLink to="/orders" class="back">← Orders</RouterLink>
  <div class="page-heading">
    <div>
      <p class="eyebrow">Ordering / Order details</p>
      <h1>Order outcome</h1>
    </div>
    <button class="secondary" :disabled="busy" @click="refresh(true)">
      Refresh status
    </button>
  </div>
  <ErrorMessage :message="error" /><LoadingState
    v-if="busy && !order"
  /><OrderSummary v-if="order" :order="order" />
  <p v-if="timedOut" role="status" class="muted">
    This order is still pending. Automatic refresh has paused; use Refresh
    status to check again.
  </p>
</template>
