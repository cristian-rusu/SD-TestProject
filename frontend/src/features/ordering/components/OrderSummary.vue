<script setup lang="ts">
import type { OrderDto } from "../types/ordering";
import StatusBadge from "../../../shared/components/StatusBadge.vue";
defineProps<{ order: OrderDto }>();
</script>
<template>
  <section class="card">
    <div aria-live="polite">
      <StatusBadge :status="order.status" />
      <p v-if="order.status === 'Pending'">
        Waiting for stock reservation result…
      </p>
      <p v-else-if="order.status === 'Confirmed'">
        Order confirmed. Stock has been reserved.
      </p>
      <p v-else>Order rejected. Stock could not be reserved.</p>
    </div>
    <dl>
      <dt>Order ID</dt>
      <dd class="mono">{{ order.orderId }}</dd>
    </dl>
    <div class="table-scroll">
      <table>
        <caption>
          Order lines
        </caption>
        <thead>
          <tr>
            <th scope="col">Product ID</th>
            <th scope="col">Quantity</th>
            <th scope="col">Accepted price</th>
            <th scope="col">Next step</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(line, index) in order.lines" :key="index">
            <td>
              <RouterLink class="mono" :to="`/catalog/${line.productId}`">{{
                line.productId
              }}</RouterLink>
            </td>
            <td>{{ line.quantity }}</td>
            <td>{{ line.acceptedPrice.toFixed(2) }}</td>
            <td>
              <RouterLink
                :to="{
                  path: '/inventory',
                  query: { productId: line.productId },
                }"
                >Check stock →</RouterLink
              >
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </section>
</template>
