import { api } from './client'

export type NotificationType = 'Follow' | 'Like' | 'Comment'

export interface Notification {
  id: string
  type: NotificationType
  actorId: string
  actorUsername: string
  logId: string | null
  igdbId: number | null
  gameName: string | null
  isRead: boolean
  createdAt: string
}

export const getNotifications = (limit = 30) =>
  api.get<Notification[]>('/api/notifications', { params: { limit } }).then((r) => r.data)

export const getUnreadCount = () =>
  api.get<number>('/api/notifications/unread-count').then((r) => r.data)

export const markAllRead = () => api.post('/api/notifications/read')
