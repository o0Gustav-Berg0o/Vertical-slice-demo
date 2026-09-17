// Expects window.BACKEND = { name, description, url } to be set before this script loads.
const BACKEND = window.BACKEND;

const state = {
  token: null, // { token, expiresAtUtc, username, role }
  products: [],
  lineItemCount: 0,
};

function apiUrl(path) {
  return `${BACKEND.url}${path}`;
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

async function apiFetch(path, options = {}) {
  const headers = { "Content-Type": "application/json", ...(options.headers || {}) };
  if (state.token) {
    headers.Authorization = `Bearer ${state.token.token}`;
  }

  const response = await fetch(apiUrl(path), { ...options, headers });

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
    if (response.status === 401) {
      state.token = null;
      renderAuthPanel();
    }

    const detail =
      (body && (body.detail || body.title)) ||
      (response.status === 401 ? "Not authenticated" : `HTTP ${response.status}`);
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

function renderBackendInfo() {
  document.getElementById("backend-name").textContent = BACKEND.name;
  document.getElementById("backend-description").textContent = BACKEND.description;
  document.getElementById("backend-url").textContent = BACKEND.url;
  pingBackend();
}

async function pingBackend() {
  const dot = document.getElementById("backend-status");
  if (!dot) return;
  try {
    // Any response (even 401) means the server is reachable.
    await fetch(`${BACKEND.url}/api/products`);
    dot.classList.add("online");
    dot.classList.remove("offline");
  } catch {
    dot.classList.remove("online");
    dot.classList.add("offline");
  }
}

function renderAuthPanel() {
  const status = document.getElementById("auth-status");
  const protectedArea = document.getElementById("protected-area");

  if (state.token) {
    status.textContent = `signed in as ${state.token.username} (${state.token.role}) (expires ${new Date(state.token.expiresAtUtc).toLocaleTimeString()})`;
    status.classList.add("online");
    protectedArea.hidden = false;
  } else {
    status.textContent = "not signed in";
    status.classList.remove("online");
    protectedArea.hidden = true;
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
    log(`Failed to load products: ${err.message}`, "error");
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
  document.getElementById("login-form").addEventListener("submit", async (e) => {
    e.preventDefault();
    const request = {
      username: document.getElementById("login-username").value,
      password: document.getElementById("login-password").value,
    };

    try {
      const result = await apiFetch("/api/auth/login", {
        method: "POST",
        body: JSON.stringify(request),
      });
      state.token = {
        token: result.token,
        expiresAtUtc: result.expiresAtUtc,
        username: result.username,
        role: result.role,
      };
      renderAuthPanel();
      log(`Signed in as ${result.username} (${result.role})`, "success");
      await loadProducts();
    } catch (err) {
      log(`Login failed: ${err.message}`, "error");
    }
  });

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
      log(`Created product #${product.id} "${product.name}"`, "success");
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
        `Created order #${order.id} for ${order.customerName} (total ${order.totalAmount.toFixed(2)})`,
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
      log(`Fetched order #${id}`, "success");
    } catch (err) {
      view.innerHTML = `<p class="muted">${escapeHtml(err.message)}</p>`;
      log(`Fetch order #${id} failed: ${err.message}`, "error");
    }
  });
}

renderBackendInfo();
renderAuthPanel();
setupForms();
addLineItem();
