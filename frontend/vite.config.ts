import { defineConfig } from "vitest/config";
import vue from "@vitejs/plugin-vue";

export default defineConfig({
  plugins: [vue()],
  server: {
    proxy: Object.fromEntries(
      ["/catalog/products", "/inventory/products", "/orders"].map((path) => [
        path,
        { target: "http://localhost:5080", changeOrigin: true },
      ]),
    ),
  },
  test: { environment: "jsdom", clearMocks: true },
});
