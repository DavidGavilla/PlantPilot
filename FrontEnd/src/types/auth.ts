export interface AuthUser {
  userId: number
  name: string
  lastName: string
  email: string
  phoneNumber?: string | null
}

export interface RegisterPayload {
  name: string
  lastName: string
  email: string
  password: string
  phoneNumber?: string
}

export interface LoginPayload {
  email: string
  password: string
}
