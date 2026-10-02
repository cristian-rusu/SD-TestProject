<template>
  <a class="skip-link" href="#main">Skip to content</a>
  <header class="site-header">
    <div class="shell header-inner">
      <RouterLink to="/catalog" class="brand"
        ><span class="brand-icon">W</span> Webshop
        <span class="muted brand-caption">/ Operations</span></RouterLink
      ><span class="environment">Modular monolith</span>
    </div>
  </header>
  <div class="shell workspace">
    <aside>
      <p class="eyebrow">Workspace</p>
      <nav aria-label="Main navigation">
        <RouterLink to="/catalog">Catalog</RouterLink
        ><RouterLink to="/inventory">Inventory</RouterLink
        ><RouterLink to="/orders">Orders</RouterLink>
      </nav>
      <div class="workflow">
        <p class="eyebrow">Demo workflow</p>
        <ol>
          <li>Create & publish</li>
          <li>Replenish stock</li>
          <li>Place an order</li>
          <li>Check the outcome</li>
        </ol>
      </div>
    </aside>
    <main id="main" tabindex="-1"><slot /></main>
  </div>
</template>
