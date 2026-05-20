import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { createMemoryRouter, RouterProvider } from 'react-router-dom'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { LoginPage } from '@/pages/LoginPage'

vi.mock('sonner', () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
  },
}))

vi.mock('@/services/auth', () => ({
  loginUser: vi.fn(),
}))

const mockSetSession = vi.fn()

vi.mock('@/stores/useAppStore', () => ({
  useAppStore: (
    selector: (state: {
      accessToken: string | null
      setSession: typeof mockSetSession
    }) => unknown,
  ) =>
    selector({
      accessToken: null,
      setSession: mockSetSession,
    }),
}))

import { loginUser } from '@/services/auth'
import { toast } from 'sonner'

const mockedLogin = vi.mocked(loginUser)
const mockedToast = vi.mocked(toast)

function renderLogin(initialPath = '/login') {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  })

  const router = createMemoryRouter(
    [
      { path: '/login', element: <LoginPage /> },
      { path: '/dashboard', element: <div>Dashboard stub</div> },
      { path: '/cadastro', element: <div>Cadastro stub</div> },
    ],
    { initialEntries: [initialPath] },
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

describe('LoginPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockedLogin.mockResolvedValue({
      userId: '00000000-0000-0000-0000-000000000001',
      name: 'Jane',
      email: 'jane@example.com',
      roles: [],
      accessToken: 'access',
      accessTokenExpiresAt: new Date().toISOString(),
      refreshToken: 'refresh',
      refreshTokenExpiresAt: new Date().toISOString(),
    })
  })

  it('renders login form', () => {
    renderLogin()

    expect(screen.getByRole('heading', { name: /entrar/i })).toBeInTheDocument()
    expect(screen.getByLabelText(/^e-mail$/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/^senha$/i)).toBeInTheDocument()
  })

  it('shows validation errors when fields are empty', async () => {
    const { user } = renderLogin()

    await user.click(screen.getByRole('button', { name: /^entrar$/i }))

    expect(await screen.findByText(/e-mail é obrigatório/i)).toBeInTheDocument()
    expect(screen.getByText(/senha é obrigatória/i)).toBeInTheDocument()
  })

  it('submits, saves session and redirects to dashboard', async () => {
    const { user, router } = renderLogin()

    await user.type(screen.getByLabelText(/^e-mail$/i), 'jane@example.com')
    await user.type(screen.getByLabelText(/^senha$/i), 'Strong1!x')
    await user.click(screen.getByRole('button', { name: /^entrar$/i }))

    expect(mockedLogin).toHaveBeenCalledWith({
      email: 'jane@example.com',
      password: 'Strong1!x',
    })

    expect(mockSetSession).toHaveBeenCalledWith({
      accessToken: 'access',
      refreshToken: 'refresh',
      user: {
        id: '00000000-0000-0000-0000-000000000001',
        name: 'Jane',
        email: 'jane@example.com',
      },
    })

    expect(await screen.findByText(/dashboard stub/i)).toBeInTheDocument()
    expect(router.state.location.pathname).toBe('/dashboard')
    expect(mockedToast.success).toHaveBeenCalled()
  })
})
