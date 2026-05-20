import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { createMemoryRouter, RouterProvider } from 'react-router-dom'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { ResetPasswordPage } from '@/pages/ResetPasswordPage'

vi.mock('sonner', () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
  },
}))

vi.mock('@/services/auth', () => ({
  resetPassword: vi.fn(),
}))

import { resetPassword } from '@/services/auth'

const mockedReset = vi.mocked(resetPassword)

const sampleUserId = '123e4567-e89b-42d3-a456-426614174000'

function renderReset(initialEntries: string[]) {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  })

  const router = createMemoryRouter(
    [
      { path: '/redefinir-senha', element: <ResetPasswordPage /> },
      { path: '/login', element: <div>Login stub</div> },
      { path: '/esqueci-senha', element: <div>Forgot stub</div> },
    ],
    { initialEntries },
  )

  return {
    user: userEvent.setup(),
    ...render(
      <QueryClientProvider client={queryClient}>
        <RouterProvider router={router} />
      </QueryClientProvider>,
    ),
    router,
  }
}

describe('ResetPasswordPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockedReset.mockResolvedValue(undefined)
  })

  it('shows link error when token is missing', async () => {
    const { router } = renderReset([`/redefinir-senha?userId=${sampleUserId}`])

    expect(await screen.findByRole('alert')).toBeInTheDocument()

    /** Guard: submitting should not reach API with invalid parameters. */
    const submit = screen.getByRole('button', { name: /salvar nova senha/i })
    expect(submit).toBeDisabled()

    /** Still on reset route (no accidental navigation before valid submit). */
    expect(router.state.location.pathname).toBe('/redefinir-senha')
  })

  it('submits reset with query params then navigates to login', async () => {
    const { user, router } = renderReset([
      `/redefinir-senha?userId=${encodeURIComponent(sampleUserId)}&token=${encodeURIComponent('identity-token-sample')}`,
    ])

    await user.type(screen.getByLabelText(/^nova senha$/i), 'Strong1!x')
    await user.type(screen.getByLabelText(/confirmar nova senha/i), 'Strong1!x')
    await user.click(screen.getByRole('button', { name: /salvar nova senha/i }))

    await vi.waitFor(() => {
      expect(mockedReset).toHaveBeenCalledWith({
        userId: sampleUserId,
        token: 'identity-token-sample',
        password: 'Strong1!x',
        confirmPassword: 'Strong1!x',
      })
    })

    await vi.waitFor(() => {
      expect(screen.getByText(/login stub/i)).toBeInTheDocument()
    })

    expect(router.state.location.pathname).toBe('/login')
  })
})
