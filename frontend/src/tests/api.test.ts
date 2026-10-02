import { afterEach, describe, expect, it, vi } from "vitest";
import { apiRequest } from "../shared/api/apiClient";
import { ApiError } from "../shared/api/apiError";
import { createProduct } from "../features/catalog/api/catalogApi";
afterEach(() => vi.unstubAllGlobals());
describe("HTTP transport", () => {
  it.each([400, 404, 409, 422, 500])("surfaces HTTP %s", async (status) => {
    vi.stubGlobal(
      "fetch",
      vi
        .fn()
        .mockResolvedValue(
          new Response(
            JSON.stringify({
              detail: "Backend explanation",
              errors: { name: ["Name is required."] },
            }),
            { status },
          ),
        ),
    );
    await expect(apiRequest("/test")).rejects.toMatchObject({
      status,
      message: "Backend explanation Name is required.",
    });
  });
  it("handles empty errors, network failures, and 204", async () => {
    const fetch = vi
      .fn()
      .mockResolvedValueOnce(new Response(null, { status: 404 }))
      .mockRejectedValueOnce(new TypeError("offline"))
      .mockResolvedValueOnce(new Response(null, { status: 204 }));
    vi.stubGlobal("fetch", fetch);
    await expect(apiRequest("/test")).rejects.toThrow("not found");
    await expect(apiRequest("/test")).rejects.toBeInstanceOf(ApiError);
    await expect(apiRequest("/test")).resolves.toBeUndefined();
  });
  it("uses the actual create-product contract", async () => {
    const fetch = vi
      .fn()
      .mockResolvedValue(
        new Response(JSON.stringify({ productId: "id" }), { status: 201 }),
      );
    vi.stubGlobal("fetch", fetch);
    await expect(
      createProduct({ name: "Product", description: "", price: 2 }),
    ).resolves.toEqual({ productId: "id" });
    expect(fetch.mock.calls[0]?.[0]).toBe("/catalog/products/");
    expect(JSON.parse(fetch.mock.calls[0]?.[1].body)).toEqual({
      name: "Product",
      description: "",
      price: 2,
    });
  });
});
