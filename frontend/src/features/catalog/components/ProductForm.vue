<script setup lang="ts">
import { reactive, ref } from "vue";
import type { CreateProductRequest } from "../types/catalog";
defineProps<{ busy: boolean }>();
const emit = defineEmits<{ submit: [value: CreateProductRequest] }>();
const form = reactive({ name: "", description: "", price: "" });
const errors = ref<Record<string, string>>({});
function submit() {
  errors.value = {};
  if (!form.name.trim()) errors.value.name = "Enter a product name.";
  if (form.price === "" || !Number.isFinite(Number(form.price)))
    errors.value.price = "Enter a numeric price.";
  if (!Object.keys(errors.value).length)
    emit("submit", {
      name: form.name.trim(),
      description: form.description,
      price: Number(form.price),
    });
}
</script>
<template>
  <form class="stack" novalidate @submit.prevent="!busy && submit()">
    <div>
      <label for="name">Name</label
      ><input
        id="name"
        v-model="form.name"
        required
        :aria-invalid="!!errors.name"
        aria-describedby="name-error"
      />
      <p id="name-error" class="field-error">{{ errors.name }}</p>
    </div>
    <div>
      <label for="description"
        >Description <span class="muted">(optional)</span></label
      ><textarea id="description" v-model="form.description" rows="4" />
    </div>
    <div>
      <label for="price">Price</label
      ><input
        id="price"
        v-model="form.price"
        type="number"
        step="any"
        required
        :aria-invalid="!!errors.price"
        aria-describedby="price-error"
      />
      <p id="price-error" class="field-error">{{ errors.price }}</p>
    </div>
    <div>
      <button :disabled="busy">
        {{ busy ? "Creating…" : "Create product" }}
      </button>
    </div>
  </form>
</template>
