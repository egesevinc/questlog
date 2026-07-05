import { api } from './client'

export interface UserSummary {
  id: string
  username: string
  avatarUrl: string | null
}

export interface UserProfile {
  id: string
  username: string
  bio: string | null
  avatarUrl: string | null
}

export interface UpdateProfileRequest {
  bio: string | null
  avatarUrl: string | null
}

export const searchUsers = (query: string) =>
  api.get<UserSummary[]>('/api/profiles/search', { params: { q: query } }).then((r) => r.data)

export const getProfile = (userId: string) =>
  api.get<UserProfile>(`/api/profiles/${userId}`).then((r) => r.data)

export const updateMyProfile = (request: UpdateProfileRequest) =>
  api.put<UserProfile>('/api/profiles/me', request).then((r) => r.data)
