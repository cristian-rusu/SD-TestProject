<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useRoute } from "vue-router";
import {
  getProduct,
  publishProduct,
  discontinueProduct,
} from "../api/catalogApi";
import type { ProductDto } from "../types/catalog";
import { errorMessage } from "../../../shared/api/apiError";
import ErrorMessage from "../../../shared/components/ErrorMessage.vue";
import LoadingState from "../../../shared/components/LoadingState.vue";
import StatusBadge from "../../../shared/components/StatusBadge.vue";
const id = String(useRoute().params.productId);
const product = ref<ProductDto>();
const busy = ref(false);
const error = ref("");
const success = ref("");
async function refresh(action?: "publish" | "discontinue") {
  if (busy.value) return;
  busy.value = true;
  error.value = "";
  success.value = "";
  try {
    if (action) {
      await (action === "publish"
        ? publishProduct(id)
        : discontinueProduct(id));
      product.value = undefined;
    }
    product.value = await getProduct(id);
    if (action) success.value = "Product updated.";
  } catch (e) {
    error.value = errorMessage(e);
  } finally {
    busy.value = false;
  }
}
onMounted(() => refresh());
</script>
<template>
  <RouterLink to="/catalog" class="back">← Catalog</RouterLink>
  <div class="page-heading">
    <div>
      <p class="eyebrow">Catalog / Product details</p>
      <h1>{{ product?.name ?? "Product" }}</h1>
    </div>
    <button class="secondary" :disabled="busy" @click="refresh()">
      Refresh
    </button>
  </div>
  <ErrorMessage :message="error" /><LoadingState v-if="busy" />
  <p role="status" class="success">{{ success }}</p>
  <section v-if="product" class="card">
    <div class="detail-top">
      <StatusBadge :status="product.status" />
      <div class="price">
        {{ product.price.toFixed(2) }}<small>Current price</small>
      </div>
    </div>
    <p>{{ product.description || "No description provided." }}</p>
    <dl>
      <dt>Product ID</dt>
      <dd class="mono">{{ product.productId }}</dd>
    </dl>
    <div class="actions">
      <button :disabled="busy" @click="refresh('publish')">Publish</button
      ><button
        class="secondary"
        :disabled="busy"
        @click="refresh('discontinue')"
      >
        Discontinue
      </button>
    </div>
  </section>
  <div v-if="product" class="next-step">
    <div>
      <p class="eyebrow">Next step</p>
      <h2>Prepare your stock</h2>
    </div>
    <RouterLink
      class="button secondary"
      :to="{ path: '/inventory', query: { productId: id } }"
      >Open inventory →</RouterLink
    ><RouterLink :to="{ path: '/orders/new', query: { productId: id } }"
      >Place order</RouterLink
    >
  </div>
</template>
