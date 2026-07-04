import { api } from './client'

export interface RegisterRequest {
  username: string
  email: string
  password: string
}

export interface LoginRequest {
  emailOrUsername: string
  password: string
}

export interface AuthResponse {
  token: string
  userId: string
  username: string
}

export const register = (request: RegisterRequest) =>
  api.post<AuthResponse>('/api/auth/register', request).then((r) => r.data)

export const login = (request: LoginRequest) =>
  api.post<AuthResponse>('/api/auth/login', request).then((r) => r.data)
