import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { createMemoryRouter, RouterProvider } from 'react-router-dom'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { HomePage } from '@/pages/HomePage'

vi.mock('@/services/health', () => ({
  fetchApiHealth: vi.fn(),
}))

import { fetchApiHealth } from '@/services/health'

const mockedFetch = vi.mocked(fetchApiHealth)

describe('HomePage', () => {
  beforeEach(() => {
    mockedFetch.mockReset()
    mockedFetch.mockResolvedValue('Healthy')
  })

  it('renders app heading', () => {
    const queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false },
      },
    })

    const router = createMemoryRouter([{ path: '/', element: <HomePage /> }], {
      initialEntries: ['/'],
    })

    render(
      <QueryClientProvider client={queryClient}>
        <RouterProvider router={router} />
      </QueryClientProvider>,
    )

    expect(
      screen.getByRole('heading', { level: 1, name: /coursely/i }),
    ).toBeInTheDocument()
  })

  it('shows API health status from health endpoint', async () => {
    const queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false },
      },
    })

    const router = createMemoryRouter([{ path: '/', element: <HomePage /> }], {
      initialEntries: ['/'],
    })

    render(
      <QueryClientProvider client={queryClient}>
        <RouterProvider router={router} />
      </QueryClientProvider>,
    )

    expect(await screen.findByText(/API health/i)).toBeInTheDocument()
    expect(await screen.findByText(/healthy/i)).toBeInTheDocument()
  })
})
