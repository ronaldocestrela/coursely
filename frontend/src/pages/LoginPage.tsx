import { Link } from 'react-router-dom'

export function LoginPage() {
  return (
    <section className="mx-auto flex min-h-dvh max-w-md flex-col justify-center gap-6 px-6 py-12 text-center">
      <h1 className="text-2xl font-semibold tracking-tight">Entrar</h1>
      <p className="text-muted-foreground text-sm">
        Login com JWT será implementado na fase 1.2 do roadmap.
      </p>
      <p className="text-sm">
        <Link to="/cadastro" className="text-primary font-medium underline-offset-4 hover:underline">
          Criar conta
        </Link>
      </p>
      <p className="text-sm">
        <Link to="/" className="text-muted-foreground underline-offset-4 hover:underline">
          Voltar ao início
        </Link>
      </p>
    </section>
  )
}
