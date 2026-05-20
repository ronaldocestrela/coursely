import { http } from '@/services/http'

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

export type LoginUserResponse = {
  userId: string
  name: string
  email: string
  roles: string[]
  accessToken: string
  accessTokenExpiresAt: string
  refreshToken: string
  refreshTokenExpiresAt: string
}

export async function loginUser(body: LoginUserDto): Promise<LoginUserResponse> {
  const { data } = await http.post<LoginUserResponse>('/api/auth/login', body)
  return data
}
