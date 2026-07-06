import axios from 'axios'
import { TOKEN_KEY, clearSession } from '../auth/storage'

export const API_BASE = import.meta.env.VITE_API_URL ?? 'https://localhost:58027'

export const api = axios.create({
  baseURL: API_BASE,
})

api.interceptors.request.use((config) => {
  const token = localStorage.getItem(TOKEN_KEY)
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// If a request fails with 401 while we believed we were logged in (a token was
// attached), the token is missing/expired/invalid — tear down the stale session
// and send the user to log in. Failed logins (no token stored yet) are left to
// the login form to surface inline.
api.interceptors.response.use(
  (response) => response,
  (error) => {
    const status = error?.response?.status
    const hadToken = Boolean(localStorage.getItem(TOKEN_KEY))
    if (status === 401 && hadToken) {
      clearSession()
      if (window.location.pathname !== '/login') {
        window.location.href = '/login'
      }
    }
    return Promise.reject(error)
  },
)
