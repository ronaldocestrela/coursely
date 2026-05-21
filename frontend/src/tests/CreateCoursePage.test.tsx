import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { createMemoryRouter, RouterProvider } from 'react-router-dom'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { CreateCoursePage } from '@/pages/CreateCoursePage'

vi.mock('sonner', () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
  },
}))

vi.mock('@/services/courses', () => ({
  createCourse: vi.fn(),
}))

import { createCourse } from '@/services/courses'
import { toast } from 'sonner'

const mockedCreate = vi.mocked(createCourse)
const mockedToast = vi.mocked(toast)

function renderCreateCourse(initialPath = '/cursos/novo') {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  })

  const router = createMemoryRouter(
    [
      { path: '/cursos/novo', element: <CreateCoursePage /> },
      { path: '/dashboard', element: <div>Dashboard stub</div> },
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

describe('CreateCoursePage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockedCreate.mockResolvedValue({
      id: '00000000-0000-0000-0000-000000000001',
      userId: '00000000-0000-0000-0000-000000000002',
      title: 'Curso',
      description: null,
      purchaseLink: null,
      thumbnailUrl: null,
      category: null,
      visibility: 'Private',
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    })
  })

  it('renders create course form', () => {
    renderCreateCourse()

    expect(screen.getByRole('heading', { name: /criar curso/i })).toBeInTheDocument()
    expect(screen.getByLabelText(/^título$/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/^descrição$/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/link de compra/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/visibilidade/i)).toBeInTheDocument()
  })

  it('shows validation when title is empty', async () => {
    const { user } = renderCreateCourse()

    await user.click(screen.getByRole('button', { name: /salvar curso/i }))

    expect(await screen.findByText(/título é obrigatório/i)).toBeInTheDocument()
  })

  it('shows validation for invalid purchase link', async () => {
    const { user } = renderCreateCourse()

    await user.type(screen.getByLabelText(/^título$/i), 'Meu curso')
    await user.type(screen.getByLabelText(/link de compra/i), 'not-a-url')
    await user.click(screen.getByRole('button', { name: /salvar curso/i }))

    expect(await screen.findByText(/url http ou https válida/i)).toBeInTheDocument()
  })

  it('submits successfully, shows toast and navigates to dashboard', async () => {
    const { user, router } = renderCreateCourse()

    await user.type(screen.getByLabelText(/^título$/i), 'Meu curso')
    await user.selectOptions(screen.getByLabelText(/visibilidade/i), 'Public')
    await user.click(screen.getByRole('button', { name: /salvar curso/i }))

    expect(mockedCreate).toHaveBeenCalledWith({
      title: 'Meu curso',
      description: undefined,
      purchaseLink: undefined,
      thumbnailUrl: undefined,
      category: undefined,
      visibility: 'Public',
    })

    expect(await screen.findByText(/dashboard stub/i)).toBeInTheDocument()
    expect(router.state.location.pathname).toBe('/dashboard')
    expect(mockedToast.success).toHaveBeenCalled()
  })

  it('disables submit button while pending', async () => {
    let resolveCreate!: (v: Awaited<ReturnType<typeof createCourse>>) => void
    mockedCreate.mockImplementation(
      () =>
        new Promise((resolve) => {
          resolveCreate = resolve
        }),
    )

    const { user } = renderCreateCourse()

    await user.type(screen.getByLabelText(/^título$/i), 'Curso')
    const button = screen.getByRole('button', { name: /salvar curso/i })
    await user.click(button)

    expect(screen.getByRole('button', { name: /salvando/i })).toBeDisabled()

    resolveCreate({
      id: '00000000-0000-0000-0000-000000000001',
      userId: '00000000-0000-0000-0000-000000000002',
      title: 'Curso',
      description: null,
      purchaseLink: null,
      thumbnailUrl: null,
      category: null,
      visibility: 'Private',
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    })

    expect(await screen.findByText(/dashboard stub/i)).toBeInTheDocument()
  })

  it('shows toast on API error', async () => {
    mockedCreate.mockRejectedValueOnce(new Error('Falha no servidor.'))

    const { user } = renderCreateCourse()

    await user.type(screen.getByLabelText(/^título$/i), 'Meu curso')
    await user.click(screen.getByRole('button', { name: /salvar curso/i }))

    await vi.waitFor(() => {
      expect(mockedToast.error).toHaveBeenCalledWith('Falha no servidor.')
    })
  })
})
