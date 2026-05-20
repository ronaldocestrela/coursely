import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'

import { Button } from '@/components/ui/button'
import { fetchApiHealth } from '@/services/health'

export function HomePage() {
  const healthQuery = useQuery({
    queryKey: ['api-health'],
    queryFn: fetchApiHealth,
  })

  return (
    <section className="mx-auto flex max-w-3xl flex-col gap-6 px-6 py-16 text-center">
      <h1 className="text-4xl font-semibold tracking-tight md:text-5xl">Coursely</h1>
      <p className="text-muted-foreground text-lg">
        Phase 0.2 — Docker stack + API health check via React Query.
      </p>

      <div
        className="bg-muted/40 rounded-lg border px-4 py-3 text-sm"
        role="status"
        aria-live="polite"
      >
        {healthQuery.isPending ? (
          <span>Checking API…</span>
        ) : healthQuery.isError ? (
          <span className="text-destructive">
            API unreachable (
            {healthQuery.error instanceof Error
              ? healthQuery.error.message
              : 'unknown error'}
            )
          </span>
        ) : (
          <span>
            API health:{' '}
            <strong className="font-medium">{healthQuery.data?.trim() ?? '—'}</strong>
          </span>
        )}
      </div>

      {healthQuery.isError ? (
        <div>
          <Button type="button" onClick={() => void healthQuery.refetch()}>
            Retry health check
          </Button>
        </div>
      ) : null}

      <div className="flex flex-wrap justify-center gap-3">
        <Button type="button" asChild>
          <Link to="/cadastro">Criar conta</Link>
        </Button>
        <Button type="button" variant="outline">
          Explore
        </Button>
      </div>
    </section>
  )
}
