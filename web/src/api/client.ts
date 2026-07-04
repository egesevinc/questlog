import axios from 'axios'

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'https://localhost:58027',
})

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('questlog_token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})
