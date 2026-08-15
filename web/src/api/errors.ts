import { isAxiosError } from 'axios'

/**
 * Turns an unknown thrown value into a user-facing message. Prefers the API's
 * `{ status, detail }` contract; otherwise says something specific about what
 * went wrong (bad response vs. never reached the server) rather than a dead-end.
 */
export function getErrorMessage(err: unknown, fallback: string): string {
  if (isAxiosError(err)) {
    const detail = err.response?.data?.detail
    if (typeof detail === 'string' && detail.trim()) return detail

    // A response came back, but not in our error shape — surface the status.
    if (err.response) return `${fallback} (HTTP ${err.response.status})`

    // No response at all — the request never reached the API.
    if (err.code === 'ERR_NETWORK') {
      return 'Could not reach the server. Is the backend running?'
    }
  }
  return fallback
}
