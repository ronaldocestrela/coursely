import { Navigate, Outlet, useLocation } from 'react-router-dom'

import { useAppStore } from '@/stores/useAppStore'

/**
 * Requires a stored access token (session). JWT validation happens on the API.
 */
export function RequireAuth() {
  const accessToken = useAppStore((s) => s.accessToken)
  const location = useLocation()

  if (!accessToken) {
    return <Navigate to="/login" replace state={{ from: location }} />
  }

  return <Outlet />
}
