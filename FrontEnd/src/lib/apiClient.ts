// Same-origin API client. The Vite dev server proxies /api/* to the backend
// (see vite.config.ts), so requests never leave the page's own origin and the
// auth cookies (httpOnly, set by the API) are attached automatically - this
// file never reads or stores a token itself.
//
// Access tokens are short-lived (15 min). On a 401, we attempt one silent
// refresh (POST /auth/refresh, which rotates the refresh cookie server-side)
// and retry the original request once. Concurrent 401s share a single
// in-flight refresh call - the backend rotates the refresh token on every
// use, so firing two refreshes at once would make the second one look like a
// replay of an already-rotated token and trigger reuse detection (forcing a
// full logout for no reason).

export class ApiError extends Error {
  status: number
  details?: unknown

  constructor(status: number, message: string, details?: unknown) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.details = details
  }
}

const NO_REFRESH_PATHS = new Set(['/auth/login', '/auth/register', '/auth/refresh'])

let inFlightRefresh: Promise<boolean> | null = null

function refreshSession(): Promise<boolean> {
  if (!inFlightRefresh) {
    inFlightRefresh = fetch('/api/auth/refresh', { method: 'POST' })
      .then((response) => response.ok)
      .catch(() => false)
      .finally(() => {
        inFlightRefresh = null
      })
  }
  return inFlightRefresh
}

async function request<T>(path: string, init?: RequestInit, isRetry = false): Promise<T> {
  const response = await fetch(`/api${path}`, {
    ...init,
    headers: {
      'Content-Type': 'application/json',
      ...init?.headers,
    },
  })

  if (response.status === 401 && !isRetry && !NO_REFRESH_PATHS.has(path)) {
    const refreshed = await refreshSession()
    if (refreshed) {
      return request<T>(path, init, true)
    }
  }

  if (response.status === 204) {
    return undefined as T
  }

  const body = await response.json().catch(() => null)

  if (!response.ok) {
    const message = extractErrorMessage(body) ?? `Request failed with status ${response.status}`
    throw new ApiError(response.status, message, body)
  }

  return body as T
}

function extractErrorMessage(body: unknown): string | undefined {
  if (body && typeof body === 'object') {
    if ('message' in body && typeof body.message === 'string') return body.message
    if ('title' in body && typeof body.title === 'string') return body.title
  }
  return undefined
}

export const apiClient = {
  get: <T>(path: string) => request<T>(path),
  post: <T>(path: string, data?: unknown) =>
    request<T>(path, { method: 'POST', body: data === undefined ? undefined : JSON.stringify(data) }),
}
