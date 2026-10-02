import { afterEach, describe, expect, it, vi } from "vitest";
import { flushPromises, mount } from "@vue/test-utils";
import { createMemoryHistory, createRouter } from "vue-router";
import CreateProductPage from "../features/catalog/pages/CreateProductPage.vue";
import OrderSummary from "../features/ordering/components/OrderSummary.vue";
import OrderDetailsPage from "../features/ordering/pages/OrderDetailsPage.vue";
import StatusBadge from "../shared/components/StatusBadge.vue";
afterEach(() => {
  vi.unstubAllGlobals();
  vi.useRealTimers();
});
const routerFor = async (path: string) => {
  const router = createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: "/:pathMatch(.*)*", component: { template: "<div />" } },
      { path: "/orders/:orderId", component: OrderDetailsPage },
    ],
  });
  await router.push(path);
  await router.isReady();
  return router;
};
it("displays Catalog backend errors and enables retry", async () => {
  vi.stubGlobal(
    "fetch",
    vi
      .fn()
      .mockResolvedValue(
        new Response(JSON.stringify({ detail: "Price must be positive." }), {
          status: 400,
        }),
      ),
  );
  const wrapper = mount(CreateProductPage, {
    global: { plugins: [await routerFor("/catalog/new")] },
  });
  await wrapper.find("#name").setValue("Product");
  await wrapper.find("#price").setValue("-1");
  await wrapper.find("form").trigger("submit");
  await flushPromises();
  expect(wrapper.find("[role=alert]").text()).toBe("Price must be positive.");
  expect(wrapper.find("button").attributes("disabled")).toBeUndefined();
  wrapper.unmount();
});
describe("Order states", () => {
  it.each(["Pending", "Confirmed", "Rejected"] as const)(
    "renders %s as accessible text with accepted price",
    (status) => {
      const wrapper = mount(OrderSummary, {
        props: {
          order: {
            orderId: "order",
            status,
            lines: [{ productId: "product", quantity: 4, acceptedPrice: 12.5 }],
          },
        },
        global: { stubs: { RouterLink: true } },
      });
      expect(wrapper.findComponent(StatusBadge).attributes("aria-label")).toBe(
        `Status: ${status}`,
      );
      expect(wrapper.text()).toContain(status);
      expect(wrapper.text()).toContain("12.50");
      expect(wrapper.find("[aria-live=polite]").exists()).toBe(true);
      if (status === "Pending")
        expect(wrapper.text()).toContain(
          "Waiting for stock reservation result",
        );
    },
  );
  it("polls Pending, stops at a final state, and clears timers on unmount", async () => {
    vi.useFakeTimers();
    const fetch = vi
      .fn()
      .mockImplementationOnce(() =>
        Promise.resolve(
          new Response(
            JSON.stringify({ orderId: "id", status: "Pending", lines: [] }),
          ),
        ),
      )
      .mockImplementation(() =>
        Promise.resolve(
          new Response(
            JSON.stringify({ orderId: "id", status: "Confirmed", lines: [] }),
          ),
        ),
      );
    vi.stubGlobal("fetch", fetch);
    const wrapper = mount(OrderDetailsPage, {
      global: { plugins: [await routerFor("/orders/id")] },
    });
    await flushPromises();
    expect(wrapper.text()).toContain("Pending");
    await vi.advanceTimersByTimeAsync(1000);
    await flushPromises();
    expect(wrapper.text()).toContain("Confirmed");
    await vi.advanceTimersByTimeAsync(5000);
    expect(fetch).toHaveBeenCalledTimes(2);
    wrapper.unmount();
    expect(vi.getTimerCount()).toBe(0);
  });
  it("pauses polling after 30 requests and permits manual refresh", async () => {
    vi.useFakeTimers();
    const fetch = vi
      .fn()
      .mockImplementation(() =>
        Promise.resolve(
          new Response(
            JSON.stringify({ orderId: "id", status: "Pending", lines: [] }),
          ),
        ),
      );
    vi.stubGlobal("fetch", fetch);
    const wrapper = mount(OrderDetailsPage, {
      global: { plugins: [await routerFor("/orders/id")] },
    });
    await flushPromises();
    await vi.advanceTimersByTimeAsync(31000);
    await flushPromises();
    expect(fetch).toHaveBeenCalledTimes(30);
    expect(wrapper.text()).toContain("Automatic refresh has paused");
    await wrapper.find("button").trigger("click");
    await flushPromises();
    expect(fetch).toHaveBeenCalledTimes(31);
    wrapper.unmount();
    await vi.advanceTimersByTimeAsync(5000);
    expect(fetch).toHaveBeenCalledTimes(31);
  });
});
