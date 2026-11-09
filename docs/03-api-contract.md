# API Contract

## Conventions
- Base path is /api.
- All endpoints except /api/auth/register and /api/auth/login require a Bearer JWT.
- Money values are decimals. Dates are calendar dates (YYYY-MM-DD).
- Every error response uses ProblemDetails.

## Endpoints summary
- Auth: POST /register, POST /login.
- Users: GET /me, PUT /me.
- Accounts: GET, POST, GET/{id}, PUT/{id}, PATCH /{id}/archive, DELETE/{id}.
- Categories: GET, POST, PUT/{id}, DELETE/{id}.
- Operations: GET (paged), POST, GET/{id}, PUT/{id}, DELETE/{id}.
- Transfers: GET, POST, GET/{id}, DELETE/{id}.
- Statistics: GET /api/statistics?currency=...&from=...&to=...

## Error codes
- 400: Validation errors.
- 401: Missing/invalid token.
- 403: Accessing another user's resource.
- 404: Resource not found.
- 409: Business rule conflict (e.g., archived account, duplicate category).