import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { createMemoryRouter, RouterProvider } from 'react-router-dom'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { RegisterPage } from '@/pages/RegisterPage'

vi.mock('sonner', () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
  },
}))

vi.mock('@/services/auth', () => ({
  registerUser: vi.fn(),
}))

import { registerUser } from '@/services/auth'
import { toast } from 'sonner'

const mockedRegister = vi.mocked(registerUser)
const mockedToast = vi.mocked(toast)

function renderRegister(initialPath = '/cadastro') {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  })

  const router = createMemoryRouter(
    [
      { path: '/cadastro', element: <RegisterPage /> },
      { path: '/login', element: <div>Login stub</div> },
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

describe('RegisterPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockedRegister.mockResolvedValue({
      id: '00000000-0000-0000-0000-000000000001',
      name: 'Jane',
      email: 'jane@example.com',
    })
  })

  it('renders registration form', () => {
    renderRegister()

    expect(screen.getByRole('heading', { name: /criar conta/i })).toBeInTheDocument()
    expect(screen.getByLabelText(/^nome$/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/^e-mail$/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/^senha$/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/confirmar senha/i)).toBeInTheDocument()
  })

  it('shows validation errors when fields are empty', async () => {
    const { user } = renderRegister()

    await user.click(screen.getByRole('button', { name: /cadastrar/i }))

    expect(await screen.findByText(/nome é obrigatório/i)).toBeInTheDocument()
    expect(screen.getByText(/e-mail é obrigatório/i)).toBeInTheDocument()
  })

  it('shows error for invalid email', async () => {
    const { user } = renderRegister()

    await user.type(screen.getByLabelText(/^nome$/i), 'Jane')
    await user.type(screen.getByLabelText(/^e-mail$/i), 'not-an-email')
    await user.type(screen.getByLabelText(/^senha$/i), 'Strong1!x')
    await user.type(screen.getByLabelText(/confirmar senha/i), 'Strong1!x')
    await user.click(screen.getByRole('button', { name: /cadastrar/i }))

    expect(await screen.findByText(/e-mail inválido/i)).toBeInTheDocument()
  })

  it('shows error for weak password', async () => {
    const { user } = renderRegister()

    await user.type(screen.getByLabelText(/^nome$/i), 'Jane')
    await user.type(screen.getByLabelText(/^e-mail$/i), 'jane@example.com')
    await user.type(screen.getByLabelText(/^senha$/i), 'weak')
    await user.type(screen.getByLabelText(/confirmar senha/i), 'weak')
    await user.click(screen.getByRole('button', { name: /cadastrar/i }))

    expect(await screen.findByText(/mínimo de 8 caracteres/i)).toBeInTheDocument()
  })

  it('shows error when passwords do not match', async () => {
    const { user } = renderRegister()

    await user.type(screen.getByLabelText(/^nome$/i), 'Jane')
    await user.type(screen.getByLabelText(/^e-mail$/i), 'jane@example.com')
    await user.type(screen.getByLabelText(/^senha$/i), 'Strong1!x')
    await user.type(screen.getByLabelText(/confirmar senha/i), 'Strong1!y')
    await user.click(screen.getByRole('button', { name: /cadastrar/i }))

    expect(await screen.findByText(/senhas não conferem/i)).toBeInTheDocument()
  })

  it('submits and redirects to login on success', async () => {
    const { user, router } = renderRegister()

    await user.type(screen.getByLabelText(/^nome$/i), 'Jane')
    await user.type(screen.getByLabelText(/^e-mail$/i), 'jane@example.com')
    await user.type(screen.getByLabelText(/^senha$/i), 'Strong1!x')
    await user.type(screen.getByLabelText(/confirmar senha/i), 'Strong1!x')
    await user.click(screen.getByRole('button', { name: /cadastrar/i }))

    expect(mockedRegister).toHaveBeenCalledWith({
      name: 'Jane',
      email: 'jane@example.com',
      password: 'Strong1!x',
    })

    expect(await screen.findByText(/login stub/i)).toBeInTheDocument()
    expect(router.state.location.pathname).toBe('/login')
    expect(mockedToast.success).toHaveBeenCalled()
  })
})
