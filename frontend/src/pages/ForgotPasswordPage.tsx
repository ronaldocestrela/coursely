import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation } from '@tanstack/react-query'
import axios from 'axios'
import { useForm } from 'react-hook-form'
import { Link } from 'react-router-dom'
import { toast } from 'sonner'

import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import {
  forgotPasswordFormSchema,
  type ForgotPasswordFormValues,
} from '@/features/auth/forgotPasswordSchema'
import { forgotPassword } from '@/services/auth'

function extractApiErrorMessage(err: unknown): string {
  if (!axios.isAxiosError(err)) {
    return err instanceof Error ? err.message : 'Erro inesperado.'
  }

  const status = err.response?.status
  const raw = err.response?.data

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

  return err.message || 'Não foi possível enviar a solicitação.'
}

export function ForgotPasswordPage() {
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<ForgotPasswordFormValues>({
    resolver: zodResolver(forgotPasswordFormSchema),
    defaultValues: { email: '' },
  })

  const mutation = useMutation({
    mutationFn: (values: ForgotPasswordFormValues) => forgotPassword({ email: values.email }),
    onSuccess: (data) => {
      toast.success(data.message)
      reset()
    },
    onError: (err) => {
      toast.error(extractApiErrorMessage(err))
    },
  })

  return (
    <section className="mx-auto flex min-h-dvh max-w-md flex-col justify-center gap-8 px-6 py-12">
      <header className="flex flex-col gap-2 text-center">
        <h1 className="text-2xl font-semibold tracking-tight">Esqueci minha senha</h1>
        <p className="text-muted-foreground text-sm">
          Informe seu e-mail. Se estiver cadastrado, você receberá instruções (em desenvolvimento, o link aparece nos
          logs da API).
        </p>
      </header>

      <form
        className="flex flex-col gap-4"
        noValidate
        onSubmit={handleSubmit((values) => mutation.mutate(values))}
      >
        <div className="flex flex-col gap-2">
          <Label htmlFor="forgot-email">E-mail</Label>
          <Input
            id="forgot-email"
            type="email"
            autoComplete="email"
            aria-invalid={Boolean(errors.email)}
            aria-describedby={errors.email ? 'forgot-email-error' : undefined}
            {...register('email')}
          />
          {errors.email ? (
            <p id="forgot-email-error" className="text-destructive text-sm" role="alert">
              {errors.email.message}
            </p>
          ) : null}
        </div>

        <Button type="submit" className="w-full" disabled={mutation.isPending}>
          {mutation.isPending ? 'Enviando…' : 'Enviar instruções'}
        </Button>
      </form>

      <p className="text-muted-foreground text-center text-sm">
        Lembrou a senha?{' '}
        <Link to="/login" className="text-primary font-medium underline-offset-4 hover:underline">
          Voltar ao login
        </Link>
      </p>

      <p className="text-center text-sm">
        <Link to="/" className="text-muted-foreground hover:text-foreground underline-offset-4 hover:underline">
          Voltar ao início
        </Link>
      </p>
    </section>
  )
}
