import { describe, expect, it } from "vitest";
import { mount } from "@vue/test-utils";
import ProductForm from "../features/catalog/components/ProductForm.vue";
import ReplenishStockForm from "../features/inventory/components/ReplenishStockForm.vue";
import PlaceOrderForm from "../features/ordering/components/PlaceOrderForm.vue";

describe("Product form", () => {
  it("connects errors to fields and emits numeric prices", async () => {
    const wrapper = mount(ProductForm, { props: { busy: false } });
    await wrapper.find("form").trigger("submit");
    expect(wrapper.find("#name").attributes("aria-invalid")).toBe("true");
    expect(wrapper.find("#name-error").text()).toContain("Enter");
    expect(wrapper.emitted("submit")).toBeUndefined();
    await wrapper.find("#name").setValue("Demo product");
    await wrapper.find("#price").setValue("12.50");
    await wrapper.find("form").trigger("submit");
    expect(wrapper.emitted("submit")?.[0]).toEqual([
      { name: "Demo product", description: "", price: 12.5 },
    ]);
    await wrapper.setProps({ busy: true });
    await wrapper.find("form").trigger("submit");
    expect(wrapper.emitted("submit")).toHaveLength(1);
    expect(wrapper.find("button").attributes("disabled")).toBeDefined();
  });
});
describe("Replenishment form", () => {
  it("requires a positive whole quantity and blocks duplicate submission", async () => {
    const wrapper = mount(ReplenishStockForm, { props: { busy: false } });
    for (const value of ["", "0", "-1", "1.5"]) {
      await wrapper.find("input").setValue(value);
      await wrapper.find("form").trigger("submit");
      expect(wrapper.emitted("submit")).toBeUndefined();
    }
    await wrapper.find("input").setValue("10");
    await wrapper.find("form").trigger("submit");
    expect(wrapper.emitted("submit")?.[0]).toEqual([10]);
    await wrapper.setProps({ busy: true });
    await wrapper.find("form").trigger("submit");
    expect(wrapper.emitted("submit")).toHaveLength(1);
  });
});
describe("Place order form", () => {
  it("validates input, carries a product ID, and only submits product and quantity", async () => {
    const id = "00000000-0000-0000-0000-000000000001";
    const wrapper = mount(PlaceOrderForm, {
      props: { busy: false, productId: id },
      global: { stubs: { RouterLink: true } },
    });
    await wrapper.find("form").trigger("submit");
    expect(wrapper.emitted("submit")).toBeUndefined();
    await wrapper.find("#order-quantity").setValue("4");
    await wrapper.find("form").trigger("submit");
    expect(wrapper.emitted("submit")?.[0]).toEqual([id, 4]);
    await wrapper.setProps({ busy: true });
    await wrapper.find("form").trigger("submit");
    expect(wrapper.emitted("submit")).toHaveLength(1);
  });
});
