import { api } from './client'

export interface GameListSummary {
  id: string
  title: string
  description: string | null
  isPublic: boolean
  itemCount: number
  createdAt: string
}

export interface GameListItem {
  id: string
  igdbId: number
  gameName: string
  coverUrl: string | null
  order: number
  note: string | null
}

export interface GameListDetail {
  id: string
  title: string
  description: string | null
  isPublic: boolean
  createdAt: string
  items: GameListItem[]
}

export interface CreateGameListRequest {
  title: string
  description: string | null
  isPublic: boolean
}

export const getMyLists = () => api.get<GameListSummary[]>('/api/lists/me').then((r) => r.data)

export const getList = (listId: string) =>
  api.get<GameListDetail>(`/api/lists/${listId}`).then((r) => r.data)

export const createList = (request: CreateGameListRequest) =>
  api.post<GameListDetail>('/api/lists', request).then((r) => r.data)

export const deleteList = (listId: string) => api.delete(`/api/lists/${listId}`)

export const addListItem = (listId: string, igdbId: number, note: string | null = null) =>
  api.post<GameListDetail>(`/api/lists/${listId}/items`, { igdbId, note }).then((r) => r.data)

export const removeListItem = (listId: string, itemId: string) =>
  api.delete(`/api/lists/${listId}/items/${itemId}`)
