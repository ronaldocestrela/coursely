import { create } from 'zustand'
import { createJSONStorage, persist } from 'zustand/middleware'

export type ThemeMode = 'light' | 'dark' | 'system'

export type AuthUser = {
  id: string
  name: string
  email: string
}

export type AppStore = {
  theme: ThemeMode
  setTheme: (theme: ThemeMode) => void

  /** Session correlation id / trace token placeholder */
  correlationId: string | null
  setCorrelationId: (correlationId: string | null) => void

  accessToken: string | null
  refreshToken: string | null
  user: AuthUser | null

  setSession: (payload: { accessToken: string; refreshToken: string; user: AuthUser }) => void
  clearSession: () => void
}

export const useAppStore = create<AppStore>()(
  persist(
    (set) => ({
      theme: 'system',
      setTheme: (theme) => set({ theme }),

      correlationId: null,
      setCorrelationId: (correlationId) => set({ correlationId }),

      accessToken: null,
      refreshToken: null,
      user: null,

      setSession: (payload) =>
        set({
          accessToken: payload.accessToken,
          refreshToken: payload.refreshToken,
          user: payload.user,
        }),

      clearSession: () =>
        set({
          accessToken: null,
          refreshToken: null,
          user: null,
        }),
    }),
    {
      name: 'coursely-storage',
      storage: createJSONStorage(() => localStorage),
      partialize: (state) => ({
        theme: state.theme,
        accessToken: state.accessToken,
        refreshToken: state.refreshToken,
        user: state.user,
      }),
    },
  ),
)
