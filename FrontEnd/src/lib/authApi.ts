import { apiClient } from './apiClient'
import type { AuthUser, LoginPayload, RegisterPayload } from '../types/auth'

// The backend sets the session as an httpOnly cookie on /register and /login -
// these calls never receive or store a token; they only return the profile.
export const authApi = {
  me: () => apiClient.get<AuthUser>('/auth/me'),
  register: (payload: RegisterPayload) => apiClient.post<AuthUser>('/auth/register', payload),
  login: (payload: LoginPayload) => apiClient.post<AuthUser>('/auth/login', payload),
  logout: () => apiClient.post<void>('/auth/logout'),
}
