<script setup lang="ts">
import { useRouter } from "vue-router";
import IdLookup from "../../../shared/components/IdLookup.vue";
import EmptyState from "../../../shared/components/EmptyState.vue";
const router = useRouter();
</script>
<template>
  <div class="page-heading">
    <div>
      <p class="eyebrow">01 / Catalog</p>
      <h1>Products</h1>
      <p class="muted">Create, publish, and manage your products.</p>
    </div>
    <RouterLink class="button" to="/catalog/new">Create product</RouterLink>
  </div>
  <section class="card">
    <h2>Find a product</h2>
    <IdLookup
      label="Product ID"
      @lookup="(id) => router.push(`/catalog/${id}`)"
    />
  </section>
  <EmptyState title="Start with a product"
    >Create a new product, or look up an existing product by ID.</EmptyState
  >
</template>
