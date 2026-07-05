import { api } from './client'
import type { LogStatus } from './logs'

export interface GameSummary {
  id: string
  igdbId: number
  name: string
  coverUrl: string | null
  releaseDate: string | null
}

export interface GameReview {
  logId: string
  userId: string
  username: string
  rating: number | null
  status: LogStatus
  body: string
  containsSpoilers: boolean
  createdAt: string
  likeCount: number
  likedByMe: boolean
}

export interface GameCommunity {
  averageRating: number | null
  logCount: number
  ratingCount: number
  reviews: GameReview[]
}

export interface GameDetail {
  id: string
  igdbId: number
  name: string
  summary: string | null
  coverUrl: string | null
  releaseDate: string | null
  genres: string[]
  platforms: string[]
}

export const searchGames = (query: string) =>
  api.get<GameSummary[]>('/api/games/search', { params: { q: query } }).then((r) => r.data)

export const getGame = (igdbId: number) =>
  api.get<GameDetail>(`/api/games/${igdbId}`).then((r) => r.data)

export const getGameCommunity = (igdbId: number) =>
  api.get<GameCommunity>(`/api/games/${igdbId}/community`).then((r) => r.data)
