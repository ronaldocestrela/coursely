import { useEffect } from 'react'
import { Outlet } from 'react-router-dom'

import { useAppStore } from '@/stores/useAppStore'

export function MainLayout() {
  const theme = useAppStore((s) => s.theme)

  useEffect(() => {
    const root = document.documentElement
    root.classList.remove('light', 'dark')

    if (theme === 'system') {
      const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches
      root.classList.add(prefersDark ? 'dark' : 'light')
      return
    }

    root.classList.add(theme)
  }, [theme])

  return (
    <div className="bg-background text-foreground min-h-dvh">
      <Outlet />
    </div>
  )
}
