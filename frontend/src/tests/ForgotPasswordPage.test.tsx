import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { createMemoryRouter, RouterProvider } from 'react-router-dom'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { ForgotPasswordPage } from '@/pages/ForgotPasswordPage'

vi.mock('sonner', () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
  },
}))

vi.mock('@/services/auth', () => ({
  forgotPassword: vi.fn(),
}))

import { forgotPassword } from '@/services/auth'
import { toast } from 'sonner'

const mockedForgot = vi.mocked(forgotPassword)
const mockedToast = vi.mocked(toast)

function renderForgot(initialPath = '/esqueci-senha') {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  })

  const router = createMemoryRouter(
    [
      { path: '/esqueci-senha', element: <ForgotPasswordPage /> },
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

describe('ForgotPasswordPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockedForgot.mockResolvedValue({
      message:
        'Se este e-mail estiver cadastrado, enviaremos instruções para redefinir a senha em instantes.',
    })
  })

  it('renders form', () => {
    renderForgot()

    expect(
      screen.getByRole('heading', { name: /esqueci minha senha/i }),
    ).toBeInTheDocument()
    expect(screen.getByLabelText(/^e-mail$/i)).toBeInTheDocument()
  })

  it('validates empty email', async () => {
    const { user } = renderForgot()

    await user.click(screen.getByRole('button', { name: /enviar instruções/i }))
    expect(await screen.findByText(/e-mail é obrigatório/i)).toBeInTheDocument()
  })

  it('submits and shows success toast', async () => {
    const { user } = renderForgot()

    await user.type(screen.getByLabelText(/^e-mail$/i), 'jane@example.com')
    await user.click(screen.getByRole('button', { name: /enviar instruções/i }))

    await vi.waitFor(() => {
      expect(mockedForgot).toHaveBeenCalledWith({ email: 'jane@example.com' })
    })

    expect(mockedToast.success).toHaveBeenCalled()
  })
})
