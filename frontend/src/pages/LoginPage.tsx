import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation } from '@tanstack/react-query'
import axios from 'axios'
import { useForm } from 'react-hook-form'
import { Link, Navigate, useLocation, useNavigate } from 'react-router-dom'
import { toast } from 'sonner'

import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { loginFormSchema, type LoginFormValues } from '@/features/auth/loginSchema'
import { loginUser } from '@/services/auth'
import { useAppStore } from '@/stores/useAppStore'

function extractApiErrorMessage(err: unknown): string {
  if (!axios.isAxiosError(err)) {
    return err instanceof Error ? err.message : 'Erro inesperado.'
  }

  const status = err.response?.status
  const raw = err.response?.data

  if (status === 401 && raw && typeof raw === 'object' && 'message' in raw) {
    const message = (raw as { message?: unknown }).message
    if (typeof message === 'string') {
      return message
    }
  }

  if (status === 400 && raw && typeof raw === 'object') {
    const errors = (raw as { errors?: Record<string, string[] | undefined> }).errors
    if (errors) {
      const first = Object.values(errors).flat().filter(Boolean)[0]
      if (typeof first === 'string') {
        return first
      }
    }
    const message = (raw as { title?: unknown }).title
    if (typeof message === 'string') {
      return message
    }
  }

  return err.message || 'Não foi possível entrar.'
}

export function LoginPage() {
  const navigate = useNavigate()
  const location = useLocation()
  const accessToken = useAppStore((s) => s.accessToken)
  const setSession = useAppStore((s) => s.setSession)

  const fromPath =
    location.state && typeof location.state === 'object' && 'from' in location.state
      ? (location.state as { from?: { pathname?: string } }).from?.pathname
      : undefined

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginFormSchema),
    defaultValues: {
      email: '',
      password: '',
    },
  })

  const mutation = useMutation({
    mutationFn: (values: LoginFormValues) => loginUser(values),
    onSuccess: (data) => {
      setSession({
        accessToken: data.accessToken,
        refreshToken: data.refreshToken,
        user: {
          id: String(data.userId),
          name: data.name,
          email: data.email,
        },
      })
      toast.success('Bem-vindo de volta!')
      navigate(fromPath && fromPath !== '/login' ? fromPath : '/dashboard', {
        replace: true,
      })
    },
    onError: (err) => {
      toast.error(extractApiErrorMessage(err))
    },
  })

  if (accessToken) {
    return <Navigate to="/dashboard" replace />
  }

  return (
    <section className="mx-auto flex min-h-dvh max-w-md flex-col justify-center gap-8 px-6 py-12">
      <header className="flex flex-col gap-2 text-center">
        <h1 className="text-2xl font-semibold tracking-tight">Entrar</h1>
        <p className="text-muted-foreground text-sm">Acesse sua conta no Coursely.</p>
      </header>

      <form
        className="flex flex-col gap-4"
        noValidate
        onSubmit={handleSubmit((values) => mutation.mutate(values))}
      >
        <div className="flex flex-col gap-2">
          <Label htmlFor="login-email">E-mail</Label>
          <Input
            id="login-email"
            type="email"
            autoComplete="email"
            aria-invalid={Boolean(errors.email)}
            aria-describedby={errors.email ? 'login-email-error' : undefined}
            {...register('email')}
          />
          {errors.email ? (
            <p
              id="login-email-error"
              className="text-destructive text-sm"
              role="alert"
            >
              {errors.email.message}
            </p>
          ) : null}
        </div>

        <div className="flex flex-col gap-2">
          <Label htmlFor="login-password">Senha</Label>
          <Input
            id="login-password"
            type="password"
            autoComplete="current-password"
            aria-invalid={Boolean(errors.password)}
            aria-describedby={errors.password ? 'login-password-error' : undefined}
            {...register('password')}
          />
          {errors.password ? (
            <p
              id="login-password-error"
              className="text-destructive text-sm"
              role="alert"
            >
              {errors.password.message}
            </p>
          ) : null}
        </div>

        <div className="flex justify-end">
          <Link
            to="/esqueci-senha"
            className="text-primary text-sm font-medium underline-offset-4 hover:underline"
          >
            Esqueci minha senha
          </Link>
        </div>

        <Button type="submit" className="w-full" disabled={mutation.isPending}>
          {mutation.isPending ? 'Entrando…' : 'Entrar'}
        </Button>
      </form>

      <p className="text-muted-foreground text-center text-sm">
        Não tem conta?{' '}
        <Link
          to="/cadastro"
          className="text-primary font-medium underline-offset-4 hover:underline"
        >
          Criar conta
        </Link>
      </p>

      <p className="text-center text-sm">
        <Link
          to="/"
          className="text-muted-foreground hover:text-foreground underline-offset-4 hover:underline"
        >
          Voltar ao início
        </Link>
      </p>
    </section>
  )
}
