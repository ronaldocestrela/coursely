import { useQueryClient } from '@tanstack/react-query'
import { Link, useNavigate } from 'react-router-dom'
import { toast } from 'sonner'

import { Button } from '@/components/ui/button'
import { logoutUser } from '@/services/auth'
import { useAppStore } from '@/stores/useAppStore'

export function DashboardPage() {
  const user = useAppStore((s) => s.user)
  const clearSession = useAppStore((s) => s.clearSession)

  const queryClient = useQueryClient()
  const navigate = useNavigate()

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
          onClick={async () => {
            const rt = useAppStore.getState().refreshToken

            try {
              if (rt) {
                await logoutUser(rt)
              }
            }
            catch {
              toast.error('Não foi possível encerrar a sessão no servidor. Você será desconectado aqui mesmo.')
            }
            finally {
              queryClient.clear()
              clearSession()
              navigate('/login', { replace: true })
            }
          }}
        >
          Encerrar sessão
        </Button>
      </div>
    </section>
  )
}
