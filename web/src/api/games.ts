import { api } from './client'

export interface GameSummary {
  id: string
  igdbId: number
  name: string
  coverUrl: string | null
  releaseDate: string | null
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
