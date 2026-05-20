import axios from 'axios'

import type { LoginUserResponse } from '@/types/authApi'

import { useAppStore } from '@/stores/useAppStore'

declare module 'axios' {
  export interface AxiosRequestConfig {
    /** When true, 401 responses will not trigger refresh / redirect. */
    skipAuthRetry?: boolean
    /** Internal: avoids infinite retries on rotated requests. */
    _retry401?: boolean
  }
}

const baseURL = import.meta.env.VITE_API_URL ?? 'http://localhost:5230'

const jsonHeaders = { 'Content-Type': 'application/json' as const }

/**
 * Axios instance without response interceptors; used for auth flows that must not recurse (refresh / logout).
 */
export const bareHttp = axios.create({
  baseURL,
  headers: jsonHeaders,
})

export const http = axios.create({
  baseURL,
  headers: jsonHeaders,
})

/** Serialize concurrent refreshes triggered by parallel 401s. */
let refreshInFlight: Promise<LoginUserResponse> | null = null

async function refreshAccessTokenViaApi(refreshToken: string): Promise<LoginUserResponse> {
  const { data } = await bareHttp.post<LoginUserResponse>('/api/auth/refresh', {
    refreshToken,
  })

  useAppStore.getState().setSession({
    accessToken: data.accessToken,
    refreshToken: data.refreshToken,
    user: {
      id: String(data.userId),
      name: data.name,
      email: data.email,
    },
  })

  return data
}

async function enqueueRefresh(refreshToken: string): Promise<LoginUserResponse> {
  if (!refreshInFlight) {
    refreshInFlight = refreshAccessTokenViaApi(refreshToken).finally(() => {
      refreshInFlight = null
    })
  }

  return refreshInFlight
}

function redirectToLoginForExpiredSession(): void {
  useAppStore.getState().clearSession()
  /** Full navigation clears in-memory caches (React Query, etc.). */
  if (typeof window !== 'undefined') {
    window.location.assign('/login')
  }
}

function shouldAttemptRefreshOn401(config?: { skipAuthRetry?: boolean; url?: string }): boolean {
  if (!config?.url || config.skipAuthRetry) {
    return false
  }

  /** These routes must handle 401 without refresh. */
  const suppressed = ['/api/auth/login', '/api/auth/register', '/api/auth/refresh', '/api/auth/logout']

  return !suppressed.some((path) => config.url!.includes(path))
}

http.interceptors.request.use((config) => {
  const token = useAppStore.getState().accessToken
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }

  return config
})

http.interceptors.response.use(
  (res) => res,
  async (error: unknown) => {
    if (!axios.isAxiosError(error)) {
      return Promise.reject(error)
    }

    const status = error.response?.status
    const originalRequest = error.config

    if (status !== 401 || !originalRequest || originalRequest._retry401) {
      return Promise.reject(error)
    }

    if (!shouldAttemptRefreshOn401(originalRequest)) {
      return Promise.reject(error)
    }

    const refreshToken = useAppStore.getState().refreshToken
    if (!refreshToken) {
      redirectToLoginForExpiredSession()
      return Promise.reject(error)
    }

    originalRequest._retry401 = true

    try {
      const tokens = await enqueueRefresh(refreshToken)
      originalRequest.headers.Authorization = `Bearer ${tokens.accessToken}`
      return await http(originalRequest)
    }
    catch {
      redirectToLoginForExpiredSession()
      return Promise.reject(error)
    }
  },
)
