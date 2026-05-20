import { bareHttp, http } from '@/services/http'
import type { LoginUserResponse } from '@/types/authApi'

export type { LoginUserResponse } from '@/types/authApi'

export type RegisterUserDto = {
  name: string
  email: string
  password: string
}

export type RegisterUserResponse = {
  id: string
  name: string
  email: string
}

export async function registerUser(body: RegisterUserDto): Promise<RegisterUserResponse> {
  const { data } = await http.post<RegisterUserResponse>('/api/auth/register', body)
  return data
}

export type LoginUserDto = {
  email: string
  password: string
}

export async function loginUser(body: LoginUserDto): Promise<LoginUserResponse> {
  const { data } = await http.post<LoginUserResponse>('/api/auth/login', body)
  return data
}

/**
 * Rotate refresh tokens (explicit call; interceptor uses the same backend contract internally).
 */
export async function refreshSession(refreshToken: string): Promise<LoginUserResponse> {
  const { data } = await bareHttp.post<LoginUserResponse>('/api/auth/refresh', {
    refreshToken,
  })

  return data
}

/** Revokes the supplied refresh token for the current device session. Idempotent server-side. */
export async function logoutUser(refreshToken: string): Promise<void> {
  await bareHttp.post('/api/auth/logout', { refreshToken })
}
