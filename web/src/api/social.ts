import { api } from './client'
import type { LogStatus } from './logs'

export interface FollowInfo {
  followerCount: number
  followingCount: number
  isFollowedByMe: boolean
}

export interface FeedItem {
  logId: string
  userId: string
  username: string
  igdbId: number
  gameName: string
  coverUrl: string | null
  status: LogStatus
  rating: number | null
  reviewBody: string | null
  createdAt: string
  likeCount: number
  likedByMe: boolean
}

export const likeLog = (logId: string) => api.post(`/api/logs/${logId}/like`)
export const unlikeLog = (logId: string) => api.delete(`/api/logs/${logId}/like`)

export const getFollowInfo = (userId: string) =>
  api.get<FollowInfo>(`/api/profiles/${userId}/follow-info`).then((r) => r.data)

export const followUser = (userId: string) => api.post(`/api/profiles/${userId}/follow`)

export const unfollowUser = (userId: string) => api.delete(`/api/profiles/${userId}/follow`)

export const getFeed = (limit = 30) =>
  api.get<FeedItem[]>('/api/feed', { params: { limit } }).then((r) => r.data)

export const getGlobalFeed = (limit = 30) =>
  api.get<FeedItem[]>('/api/feed/global', { params: { limit } }).then((r) => r.data)
