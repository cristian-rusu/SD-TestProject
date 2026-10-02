# Webshop frontend

Vue 3 + TypeScript + Vite, organized by Catalog, Inventory, and Ordering. The UI calls HTTP APIs only. No Pinia, replicated business rules, or persisted server-state cache.

## Development

Start PostgreSQL and the ASP.NET Core application as described in the root README, then in a second terminal:

```powershell
cd frontend
npm ci
npm run dev
```

Open the URL printed by Vite. The proxy forwards `/catalog/products`, `/inventory/products`, and `/orders` to `http://localhost:5080`. Set `VITE_API_BASE_URL` only when deliberately using another API origin; normally leave it empty. See `.env.example`.

## One production deployment

From the repository root:

```powershell
dotnet publish src/Company.Webshop.Main/Company.Webshop.Main.csproj -c Release -o artifacts/webshop -m:1
cd artifacts/webshop
dotnet Company.Webshop.Main.dll --urls http://localhost:5080
```

Publishing runs `npm ci` and `npm run build`, then includes `frontend/dist` in the application's published `wwwroot`. Deploy the complete publish directory. Node is a build prerequisite, not a production runtime dependency. Configure `ConnectionStrings__Webshop` for your deployment database.

The ASP.NET Core host serves both the UI and the unchanged API. Vue uses hash navigation (for example `/#/orders/new`) because `/orders/{guid}` already belongs to the API. No SPA fallback is needed; unknown API paths remain 404 instead of returning HTML.

## Actual API constraints

- There are no product or order list endpoints, so the landing pages provide ID lookup. Product links carry the ID into Inventory and Ordering; order lines link back to stock. Bookmark detail URLs to revisit records.
- Product responses contain `productId`, `name`, `description`, `price`, and `status`. They do not expose `isAvailableForSale`; the UI displays the supplied status without deriving sellability. Publish/discontinue decisions remain server-side.
- Creation returns `{ productId }` or `{ orderId }`, followed by a details GET. Order lines contain `productId`, `quantity`, and `acceptedPrice` (no line ID). Currency and creation date are not exposed and are not invented by the UI.
- Inventory exposes quantity only. A 404 is shown as a missing stock record, not as zero. Replenishment creates the record through the API.
- Pending orders poll at one-second intervals for up to 30 requests, then offer manual refresh. Polling stops on a terminal status, request failure, or page departure. The current awaited dispatcher usually returns a final outcome immediately; the UI never fabricates a Pending delay.

## Demo

1. Catalog → Create product → enter name, description, price.
2. Publish → Open inventory.
3. Replenish 10 → Place order → quantity 4.
4. Observe Confirmed (or Pending until the next refresh), and the accepted price.
5. Check stock → verify 6 available.
6. Place order → quantity 20 → observe Rejected.
7. Check stock → verify 6 still available.

## Checks

```powershell
npm test
npm run build
```

Vitest and Vue Test Utils cover forms, duplicate submission, API errors including Problem Details and network failures, all order statuses, accessible status text, and polling completion/timeout/cleanup.

With a published host running against a **disposable** database, `powershell -File frontend/scripts/smoke.ps1 -BaseUrl http://localhost:5180` verifies static assets, API 404s, publication, replenishment, confirmed/rejected orders, accepted price, unchanged stock after rejection, and discontinuation. It creates one product and two orders.

Validation completed: 16 frontend tests, 56 backend unit tests, 10 PostgreSQL integration tests, production publish, the HTTP smoke flow, and development proxy forwarding. Browser visual verification remains a manual check.
