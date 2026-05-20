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
