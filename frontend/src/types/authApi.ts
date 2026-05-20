/** Login & refresh payloads share this contract (camelCase JSON from ASP.NET defaults). */
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
