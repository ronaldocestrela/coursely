import { Link } from 'react-router-dom'

import { Button } from '@/components/ui/button'
import { useAppStore } from '@/stores/useAppStore'

export function DashboardPage() {
  const user = useAppStore((s) => s.user)
  const clearSession = useAppStore((s) => s.clearSession)

  return (
    <section className="mx-auto flex min-h-dvh max-w-lg flex-col justify-center gap-8 px-6 py-12">
      <header className="flex flex-col gap-2 text-center">
        <h1 className="text-2xl font-semibold tracking-tight">Dashboard</h1>
        <p className="text-muted-foreground text-sm">
          Sessão ativa como <span className="text-foreground font-medium">{user?.name ?? '—'}</span> (
          {user?.email ?? '—'})
        </p>
      </header>

      <div className="flex flex-col gap-3">
        <Button type="button" variant="outline" asChild>
          <Link to="/">Voltar ao início</Link>
        </Button>
        <Button
          type="button"
          variant="secondary"
          onClick={() => {
            clearSession()
          }}
        >
          Encerrar sessão
        </Button>
      </div>
    </section>
  )
}
