<script setup lang="ts">
import { useRouter } from "vue-router";
import IdLookup from "../../../shared/components/IdLookup.vue";
import EmptyState from "../../../shared/components/EmptyState.vue";
const router = useRouter();
</script>
<template>
  <div class="page-heading">
    <div>
      <p class="eyebrow">03 / Ordering</p>
      <h1>Orders</h1>
      <p class="muted">Place an order and follow its outcome.</p>
    </div>
    <RouterLink class="button" to="/orders/new">Place order</RouterLink>
  </div>
  <section class="card">
    <h2>Find an order</h2>
    <IdLookup label="Order ID" @lookup="(id) => router.push(`/orders/${id}`)" />
  </section>
  <EmptyState title="Follow an order"
    >Place a new order, or enter an existing order ID to see its status and
    accepted price.</EmptyState
  >
</template>
