const BACKENDS = [
  {
    id: "vsa",
    name: "Vertical Slice API",
    description: "Feature folders: Command/Handler/Validator per slice (MediatR)",
    url: "http://localhost:5010",
  },
  {
    id: "layered",
    name: "Layered API",
    description: "Controller → Service → Repository → Data",
    url: "http://localhost:5136",
  },
];

const state = {
  activeBackendId: BACKENDS[0].id,
  products: [],
  lineItemCount: 0,
};

function activeBackend() {
  return BACKENDS.find((b) => b.id === state.activeBackendId);
}

function apiUrl(path) {
  return `${activeBackend().url}${path}`;
}

function log(message, type = "info") {
  const container = document.getElementById("log-entries");
  const entry = document.createElement("div");
  entry.className = `log-entry ${type}`;
  const time = new Date().toLocaleTimeString();
  entry.textContent = `[${time}] ${message}`;
  container.prepend(entry);
  while (container.children.length > 50) {
    container.removeChild(container.lastChild);
  }
}

async function apiFetch(path, options) {
  const response = await fetch(apiUrl(path), {
    headers: { "Content-Type": "application/json" },
    ...options,
  });

  let body = null;
  const text = await response.text();
  if (text) {
    try {
      body = JSON.parse(text);
    } catch {
      body = text;
    }
  }

  if (!response.ok) {
    const detail =
      (body && (body.detail || body.title)) || `HTTP ${response.status}`;
    const errors = body && body.errors
      ? Object.entries(body.errors)
          .map(([field, msgs]) => `${field}: ${msgs.join(", ")}`)
          .join(" | ")
      : null;
    const error = new Error(errors ? `${detail} (${errors})` : detail);
    error.status = response.status;
    error.body = body;
    throw error;
  }

  return body;
}

function renderBackendSwitcher() {
  const container = document.getElementById("backend-switcher");
  container.innerHTML = "";

  for (const backend of BACKENDS) {
    const card = document.createElement("div");
    card.className = "backend-card" + (backend.id === state.activeBackendId ? " active" : "");
    card.innerHTML = `
      <h3><span class="status-dot" id="status-${backend.id}"></span>${backend.name}</h3>
      <p>${backend.description}</p>
      <div class="url">${backend.url}</div>
    `;
    card.addEventListener("click", () => {
      state.activeBackendId = backend.id;
      renderBackendSwitcher();
      loadProducts();
    });
    container.appendChild(card);
  }

  for (const backend of BACKENDS) {
    pingBackend(backend);
  }
}

async function pingBackend(backend) {
  const dot = document.getElementById(`status-${backend.id}`);
  if (!dot) return;
  try {
    const response = await fetch(`${backend.url}/api/products`);
    dot.classList.toggle("online", response.ok);
    dot.classList.toggle("offline", !response.ok);
  } catch {
    dot.classList.remove("online");
    dot.classList.add("offline");
  }
}

function renderProductsTable() {
  const body = document.getElementById("products-table-body");
  body.innerHTML = "";

  if (state.products.length === 0) {
    body.innerHTML = '<tr><td colspan="4" class="muted">No products yet.</td></tr>';
    return;
  }

  for (const product of state.products) {
    const row = document.createElement("tr");
    row.innerHTML = `
      <td>${product.id}</td>
      <td>${escapeHtml(product.name)}</td>
      <td>${product.price.toFixed(2)}</td>
      <td>${product.stockQuantity}</td>
    `;
    body.appendChild(row);
  }
}

function escapeHtml(value) {
  const div = document.createElement("div");
  div.textContent = value;
  return div.innerHTML;
}

async function loadProducts() {
  try {
    state.products = await apiFetch("/api/products");
    renderProductsTable();
    refreshLineItemProductOptions();
  } catch (err) {
    log(`Failed to load products from ${activeBackend().name}: ${err.message}`, "error");
    state.products = [];
    renderProductsTable();
  }
}

function productOptionsHtml(selectedId) {
  if (state.products.length === 0) {
    return '<option value="">No products available</option>';
  }
  return state.products
    .map(
      (p) =>
        `<option value="${p.id}" ${p.id === selectedId ? "selected" : ""}>#${p.id} ${escapeHtml(p.name)}</option>`
    )
    .join("");
}

function refreshLineItemProductOptions() {
  document.querySelectorAll(".line-item select").forEach((select) => {
    const current = Number(select.value) || null;
    select.innerHTML = productOptionsHtml(current);
  });
}

function addLineItem() {
  const container = document.getElementById("line-items");
  const id = `line-item-${state.lineItemCount++}`;
  const row = document.createElement("div");
  row.className = "line-item";
  row.id = id;
  row.innerHTML = `
    <label>Product
      <select>${productOptionsHtml(null)}</select>
    </label>
    <label>Qty
      <input type="number" min="1" step="1" value="1" />
    </label>
    <button type="button" class="secondary icon" data-remove="${id}">Remove</button>
  `;
  row.querySelector("[data-remove]").addEventListener("click", () => {
    row.remove();
  });
  container.appendChild(row);
}

function collectLineItems() {
  return Array.from(document.querySelectorAll(".line-item")).map((row) => {
    const select = row.querySelector("select");
    const qtyInput = row.querySelector("input");
    return {
      productId: Number(select.value),
      quantity: Number(qtyInput.value),
    };
  });
}

function setupForms() {
  document.getElementById("product-form").addEventListener("submit", async (e) => {
    e.preventDefault();
    const request = {
      name: document.getElementById("product-name").value,
      description: document.getElementById("product-description").value,
      price: Number(document.getElementById("product-price").value),
      stockQuantity: Number(document.getElementById("product-stock").value),
    };

    try {
      const product = await apiFetch("/api/products", {
        method: "POST",
        body: JSON.stringify(request),
      });
      log(`Created product #${product.id} "${product.name}" via ${activeBackend().name}`, "success");
      e.target.reset();
      await loadProducts();
    } catch (err) {
      log(`Create product failed: ${err.message}`, "error");
    }
  });

  document.getElementById("refresh-products").addEventListener("click", loadProducts);

  document.getElementById("add-line-item").addEventListener("click", addLineItem);

  document.getElementById("order-form").addEventListener("submit", async (e) => {
    e.preventDefault();
    const items = collectLineItems();

    if (items.length === 0) {
      log("Add at least one line item before creating an order.", "error");
      return;
    }

    const request = {
      customerName: document.getElementById("order-customer").value,
      items,
    };

    try {
      const order = await apiFetch("/api/orders", {
        method: "POST",
        body: JSON.stringify(request),
      });
      log(
        `Created order #${order.id} for ${order.customerName} (total ${order.totalAmount.toFixed(2)}) via ${activeBackend().name}`,
        "success"
      );
      e.target.reset();
      document.getElementById("line-items").innerHTML = "";
      document.getElementById("order-lookup-id").value = order.id;
    } catch (err) {
      log(`Create order failed: ${err.message}`, "error");
    }
  });

  document.getElementById("order-lookup-form").addEventListener("submit", async (e) => {
    e.preventDefault();
    const id = document.getElementById("order-lookup-id").value;
    const view = document.getElementById("order-view");

    try {
      const order = await apiFetch(`/api/orders/${id}`);
      view.innerHTML = `<pre>${escapeHtml(JSON.stringify(order, null, 2))}</pre>`;
      log(`Fetched order #${id} via ${activeBackend().name}`, "success");
    } catch (err) {
      view.innerHTML = `<p class="muted">${escapeHtml(err.message)}</p>`;
      log(`Fetch order #${id} failed: ${err.message}`, "error");
    }
  });
}

renderBackendSwitcher();
setupForms();
addLineItem();
loadProducts();
