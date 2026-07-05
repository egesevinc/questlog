import { api } from './client'

export type LogStatus = 'Wishlist' | 'Backlog' | 'Playing' | 'Completed' | 'Abandoned' | 'Replaying'

export interface CreateGameLogRequest {
  igdbId: number
  status: LogStatus
  rating: number | null
  hoursPlayed: number | null
  startedAt: string | null
  finishedAt: string | null
  reviewBody?: string | null
  containsSpoilers?: boolean
}

export type UpdateGameLogRequest = Omit<CreateGameLogRequest, 'igdbId'>

export interface GameLog {
  id: string
  igdbId: number
  gameName: string
  coverUrl: string | null
  status: LogStatus
  rating: number | null
  hoursPlayed: number | null
  startedAt: string | null
  finishedAt: string | null
  createdAt: string
  reviewBody: string | null
  containsSpoilers: boolean
}

export interface GenreCount {
  genre: string
  count: number
}

export interface ProfileStats {
  totalLogged: number
  completed: number
  playing: number
  backlog: number
  abandoned: number
  averageRating: number | null
  topGenres: GenreCount[]
}

export const createLog = (request: CreateGameLogRequest) =>
  api.post<GameLog>('/api/logs', request).then((r) => r.data)

export const updateLog = (logId: string, request: UpdateGameLogRequest) =>
  api.put<GameLog>(`/api/logs/${logId}`, request).then((r) => r.data)

export const deleteLog = (logId: string) => api.delete(`/api/logs/${logId}`)

export const getMyLogs = () => api.get<GameLog[]>('/api/logs/me').then((r) => r.data)

// The current user's log for a game, or null if they haven't logged it (404).
export const getMyLogForGame = (igdbId: number) =>
  api
    .get<GameLog>(`/api/logs/game/${igdbId}`)
    .then((r) => r.data)
    .catch((err) => {
      if (err?.response?.status === 404) return null
      throw err
    })

export const getUserLogs = (userId: string) =>
  api.get<GameLog[]>(`/api/profiles/${userId}/logs`).then((r) => r.data)

export const getUserStats = (userId: string) =>
  api.get<ProfileStats>(`/api/profiles/${userId}/stats`).then((r) => r.data)

export interface Comment {
  id: string
  userId: string
  username: string
  body: string
  createdAt: string
}

export interface LogDetail {
  id: string
  userId: string
  username: string
  igdbId: number
  gameName: string
  coverUrl: string | null
  status: LogStatus
  rating: number | null
  hoursPlayed: number | null
  reviewBody: string | null
  containsSpoilers: boolean
  createdAt: string
  likeCount: number
  likedByMe: boolean
  comments: Comment[]
}

export const getLogDetail = (logId: string) =>
  api.get<LogDetail>(`/api/logs/${logId}`).then((r) => r.data)

export const addComment = (logId: string, body: string) =>
  api.post<Comment>(`/api/logs/${logId}/comments`, { body }).then((r) => r.data)

export const deleteComment = (commentId: string) =>
  api.delete(`/api/logs/comments/${commentId}`)
