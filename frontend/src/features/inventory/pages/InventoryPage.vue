<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import { getStock, replenishStock } from "../api/inventoryApi";
import type { StockDto } from "../types/inventory";
import { ApiError, errorMessage } from "../../../shared/api/apiError";
import IdLookup from "../../../shared/components/IdLookup.vue";
import ErrorMessage from "../../../shared/components/ErrorMessage.vue";
import LoadingState from "../../../shared/components/LoadingState.vue";
import EmptyState from "../../../shared/components/EmptyState.vue";
import ReplenishStockForm from "../components/ReplenishStockForm.vue";
const router = useRouter();
const id = String(useRoute().query.productId ?? "");
const stock = ref<StockDto>();
const busy = ref(false);
const error = ref("");
const success = ref("");
const missing = ref(false);
async function load(quantity?: number) {
  if (busy.value || !id) return;
  busy.value = true;
  error.value = "";
  success.value = "";
  missing.value = false;
  stock.value = undefined;
  try {
    if (quantity !== undefined) await replenishStock(id, quantity);
    stock.value = await getStock(id);
    if (quantity !== undefined)
      success.value = "Stock replenished. The quantity below is up to date.";
  } catch (e) {
    if (e instanceof ApiError && e.status === 404 && quantity === undefined)
      missing.value = true;
    else error.value = errorMessage(e);
  } finally {
    busy.value = false;
  }
}
onMounted(() => load());
</script>
<template>
  <div class="page-heading">
    <div>
      <p class="eyebrow">02 / Inventory</p>
      <h1>Stock control</h1>
      <p class="muted">View availability and replenish a product.</p>
    </div>
    <button v-if="id" class="secondary" :disabled="busy" @click="load()">
      Refresh stock
    </button>
  </div>
  <section class="card">
    <IdLookup
      label="Product ID"
      :busy="busy"
      @lookup="
        (productId) => router.push({ path: '/inventory', query: { productId } })
      "
    />
  </section>
  <ErrorMessage :message="error" /><LoadingState v-if="busy" />
  <p class="success" role="status">{{ success }}</p>
  <template v-if="id"
    ><section class="card">
      <p class="eyebrow">Product reference</p>
      <p class="mono">{{ id }}</p>
      <div v-if="stock" class="stock-count">
        {{ stock.availableQuantity }}<small>Available units</small>
      </div>
      <p v-if="missing" role="status">
        No stock record yet. Replenish this product to create one.
      </p>
      <ReplenishStockForm :busy="busy" @submit="load" />
    </section>
    <div class="next-step">
      <RouterLink :to="`/catalog/${id}`">View product</RouterLink
      ><RouterLink
        class="button"
        :to="{ path: '/orders/new', query: { productId: id } }"
        >Place order →</RouterLink
      >
    </div></template
  ><EmptyState v-else title="Find your stock"
    >Open inventory from a product, or enter a product ID above.</EmptyState
  >
</template>
