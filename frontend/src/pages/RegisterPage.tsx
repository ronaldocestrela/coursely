import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation } from '@tanstack/react-query'
import axios from 'axios'
import { useForm } from 'react-hook-form'
import { Link, useNavigate } from 'react-router-dom'
import { toast } from 'sonner'

import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import {
  registerFormSchema,
  type RegisterFormValues,
} from '@/features/auth/registerSchema'
import { registerUser } from '@/services/auth'

function extractApiErrorMessage(err: unknown): string {
  if (!axios.isAxiosError(err)) {
    return err instanceof Error ? err.message : 'Erro inesperado.'
  }

  const status = err.response?.status
  const raw = err.response?.data

  if (status === 409 && raw && typeof raw === 'object' && 'message' in raw) {
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

  return err.message || 'Não foi possível concluir o cadastro.'
}

export function RegisterPage() {
  const navigate = useNavigate()
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<RegisterFormValues>({
    resolver: zodResolver(registerFormSchema),
    defaultValues: {
      name: '',
      email: '',
      password: '',
      confirmPassword: '',
    },
  })

  const mutation = useMutation({
    mutationFn: (values: RegisterFormValues) =>
      registerUser({
        name: values.name,
        email: values.email,
        password: values.password,
      }),
    onSuccess: () => {
      toast.success('Conta criada! Faça login para continuar.')
      navigate('/login', { replace: true })
    },
    onError: (err) => {
      toast.error(extractApiErrorMessage(err))
    },
  })

  return (
    <section className="mx-auto flex min-h-dvh max-w-md flex-col justify-center gap-8 px-6 py-12">
      <header className="flex flex-col gap-2 text-center">
        <h1 className="text-2xl font-semibold tracking-tight">Criar conta</h1>
        <p className="text-muted-foreground text-sm">
          Preencha os dados abaixo para começar no Coursely.
        </p>
      </header>

      <form
        className="flex flex-col gap-4"
        noValidate
        onSubmit={handleSubmit((values) => mutation.mutate(values))}
      >
        <div className="flex flex-col gap-2">
          <Label htmlFor="register-name">Nome</Label>
          <Input
            id="register-name"
            type="text"
            autoComplete="name"
            aria-invalid={Boolean(errors.name)}
            aria-describedby={errors.name ? 'register-name-error' : undefined}
            {...register('name')}
          />
          {errors.name ? (
            <p
              id="register-name-error"
              className="text-destructive text-sm"
              role="alert"
            >
              {errors.name.message}
            </p>
          ) : null}
        </div>

        <div className="flex flex-col gap-2">
          <Label htmlFor="register-email">E-mail</Label>
          <Input
            id="register-email"
            type="email"
            autoComplete="email"
            aria-invalid={Boolean(errors.email)}
            aria-describedby={errors.email ? 'register-email-error' : undefined}
            {...register('email')}
          />
          {errors.email ? (
            <p
              id="register-email-error"
              className="text-destructive text-sm"
              role="alert"
            >
              {errors.email.message}
            </p>
          ) : null}
        </div>

        <div className="flex flex-col gap-2">
          <Label htmlFor="register-password">Senha</Label>
          <Input
            id="register-password"
            type="password"
            autoComplete="new-password"
            aria-invalid={Boolean(errors.password)}
            aria-describedby={errors.password ? 'register-password-error' : undefined}
            {...register('password')}
          />
          {errors.password ? (
            <p
              id="register-password-error"
              className="text-destructive text-sm"
              role="alert"
            >
              {errors.password.message}
            </p>
          ) : null}
          <p className="text-muted-foreground text-xs">
            Mínimo 8 caracteres, com maiúscula, minúscula, número e caractere
            especial.
          </p>
        </div>

        <div className="flex flex-col gap-2">
          <Label htmlFor="register-confirm">Confirmar senha</Label>
          <Input
            id="register-confirm"
            type="password"
            autoComplete="new-password"
            aria-invalid={Boolean(errors.confirmPassword)}
            aria-describedby={
              errors.confirmPassword ? 'register-confirm-error' : undefined
            }
            {...register('confirmPassword')}
          />
          {errors.confirmPassword ? (
            <p
              id="register-confirm-error"
              className="text-destructive text-sm"
              role="alert"
            >
              {errors.confirmPassword.message}
            </p>
          ) : null}
        </div>

        <Button type="submit" className="w-full" disabled={mutation.isPending}>
          {mutation.isPending ? 'Criando conta…' : 'Cadastrar'}
        </Button>
      </form>

      <p className="text-muted-foreground text-center text-sm">
        Já tem uma conta?{' '}
        <Link
          to="/login"
          className="text-primary font-medium underline-offset-4 hover:underline"
        >
          Entrar
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
