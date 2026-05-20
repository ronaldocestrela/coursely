import { bareHttp, http } from '@/services/http'
import type { ForgotPasswordResponseDto, LoginUserResponse } from '@/types/authApi'

export type { ForgotPasswordResponseDto, LoginUserResponse } from '@/types/authApi'

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

export type ForgotPasswordDto = {
  email: string
}

/** Always returns generic success wording (enumeration-safe). */
export async function forgotPassword(body: ForgotPasswordDto): Promise<ForgotPasswordResponseDto> {
  const { data } = await http.post<ForgotPasswordResponseDto>('/api/auth/forgot-password', body)
  return data
}

export type ResetPasswordDto = {
  userId: string
  token: string
  password: string
  confirmPassword: string
}

/** Completes ASP.NET Identity password reset (HTTP 204 on success). */
export async function resetPassword(body: ResetPasswordDto): Promise<void> {
  await http.post('/api/auth/reset-password', {
    userId: body.userId,
    token: body.token,
    password: body.password,
    confirmPassword: body.confirmPassword,
  })
}
