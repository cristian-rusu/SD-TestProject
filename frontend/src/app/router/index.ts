import { createRouter, createWebHashHistory } from "vue-router";
export const router = createRouter({
  history: createWebHashHistory(),
  routes: [
    { path: "/", redirect: "/catalog" },
    {
      path: "/catalog",
      component: () => import("../../features/catalog/pages/CatalogPage.vue"),
    },
    {
      path: "/catalog/new",
      component: () =>
        import("../../features/catalog/pages/CreateProductPage.vue"),
    },
    {
      path: "/catalog/:productId",
      component: () =>
        import("../../features/catalog/pages/ProductDetailsPage.vue"),
    },
    {
      path: "/inventory",
      component: () =>
        import("../../features/inventory/pages/InventoryPage.vue"),
    },
    {
      path: "/orders",
      component: () => import("../../features/ordering/pages/OrdersPage.vue"),
    },
    {
      path: "/orders/new",
      component: () =>
        import("../../features/ordering/pages/PlaceOrderPage.vue"),
    },
    {
      path: "/orders/:orderId",
      component: () =>
        import("../../features/ordering/pages/OrderDetailsPage.vue"),
    },
    { path: "/:pathMatch(.*)*", redirect: "/catalog" },
  ],
});
