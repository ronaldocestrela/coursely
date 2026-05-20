import { create } from 'zustand'

export type ThemeMode = 'light' | 'dark' | 'system'

export type AppStore = {
  theme: ThemeMode
  setTheme: (theme: ThemeMode) => void

  /** Session correlation id / trace token placeholder */
  correlationId: string | null
  setCorrelationId: (correlationId: string | null) => void

  /** Auth placeholder — populated in Phase 1 */
  accessToken: string | null
  setAccessToken: (accessToken: string | null) => void
}

export const useAppStore = create<AppStore>((set) => ({
  theme: 'system',
  setTheme: (theme) => set({ theme }),

  correlationId: null,
  setCorrelationId: (correlationId) => set({ correlationId }),

  accessToken: null,
  setAccessToken: (accessToken) => set({ accessToken }),
}))
