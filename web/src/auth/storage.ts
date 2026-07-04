// Single source of truth for the localStorage keys and session teardown, shared
// by the auth context and the axios client so the key names can't drift apart.
export const TOKEN_KEY = 'questlog_token'
export const USER_KEY = 'questlog_user'

export function clearSession() {
  localStorage.removeItem(TOKEN_KEY)
  localStorage.removeItem(USER_KEY)
}
