# Frontend

A dependency-free static page (plain HTML/CSS/JS, no build step, no framework) that
exercises the API end to end: register, sign in, create/list products, create/look up
orders. It exists to make the API easy to poke at without `curl`, not as a production UI —
feel free to replace it entirely once you're building your own app on this template.

```bash
cd frontend
dotnet run   # http://localhost:8080
```

`Frontend.csproj` is a minimal ASP.NET Core app whose only job is `UseStaticFiles()` —
`index.html`, `shared/app.js`, and `shared/styles.css` are served as-is, no build step.

## How it talks to the API

- `index.html` sets `window.BACKEND = { name, description, url }` in an inline `<script>`
  before loading `shared/app.js`. `url` is the API's base address (`http://localhost:5010`
  by default) — change it here if you run the API on a different port.
- The API must be running separately (see the root README) — this project only serves
  static files, it doesn't proxy or host the API itself.
- The API enables CORS (`AllowAnyOrigin`) in `Program.cs` for local development, which is
  what lets a page served from `:8080` call an API on `:5010` at all.

## Auth in the UI

- The **Sign in** panel holds both the login form and, behind a "Register a new account"
  disclosure, the registration form. Demo credentials (`admin`/`Passw0rd!`) are pre-filled
  in the login form.
- A successful login stores the returned token in memory (`state.token`) — not
  `localStorage` — so refreshing the page signs you out. This is deliberate: it keeps the
  demo simple and avoids leaving a token sitting in browser storage.
- Every API call attaches `Authorization: Bearer <token>` automatically once signed in (see
  `apiFetch` in `shared/app.js`).
- A `401` response clears the stored token and shows the sign-in panel again — the token
  expired, or was never valid.
- A `403` response (e.g. a non-Admin user calling `POST /api/products`) shows a distinct
  "Forbidden — you don't have the required role for this action" message, rather than being
  lumped in with a generic error. This is a new failure mode this template introduces on top
  of plain authentication: being *logged in* and being *allowed to do this specific thing*
  are different checks.
- New accounts (via Register) get no role, so they can browse and create orders but will see
  the 403 above if they try to create a product — log in as `admin` instead, or extend the
  app with a way for an Admin to promote another user.
