<script setup lang="ts">
import { ref } from "vue";
import { useRouter } from "vue-router";
import ProductForm from "../components/ProductForm.vue";
import { createProduct } from "../api/catalogApi";
import type { CreateProductRequest } from "../types/catalog";
import { errorMessage } from "../../../shared/api/apiError";
import ErrorMessage from "../../../shared/components/ErrorMessage.vue";
const router = useRouter();
const busy = ref(false);
const error = ref("");
async function submit(value: CreateProductRequest) {
  if (busy.value) return;
  busy.value = true;
  error.value = "";
  try {
    const result = await createProduct(value);
    await router.push(`/catalog/${result.productId}`);
  } catch (e) {
    error.value = errorMessage(e);
  } finally {
    busy.value = false;
  }
}
</script>
<template>
  <RouterLink to="/catalog" class="back">← Catalog</RouterLink>
  <div class="page-heading">
    <div>
      <p class="eyebrow">Catalog</p>
      <h1>Create product</h1>
      <p class="muted">Add the details, then publish when ready.</p>
    </div>
  </div>
  <section class="card form-card">
    <ErrorMessage :message="error" /><ProductForm
      :busy="busy"
      @submit="submit"
    />
  </section>
</template>
