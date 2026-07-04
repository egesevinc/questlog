import { api } from './client'

export interface UserSummary {
  id: string
  username: string
  avatarUrl: string | null
}

export const searchUsers = (query: string) =>
  api.get<UserSummary[]>('/api/profiles/search', { params: { q: query } }).then((r) => r.data)
