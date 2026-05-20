import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation } from '@tanstack/react-query'
import axios from 'axios'
import { useMemo } from 'react'
import { useForm } from 'react-hook-form'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { toast } from 'sonner'

import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import {
  resetPasswordFormSchema,
  resetPasswordQuerySchema,
  type ResetPasswordFormValues,
} from '@/features/auth/resetPasswordSchema'
import { resetPassword } from '@/services/auth'

function extractResetError(err: unknown): string {
  if (!axios.isAxiosError(err)) {
    return err instanceof Error ? err.message : 'Erro inesperado.'
  }

  const status = err.response?.status
  const raw = err.response?.data

  if (status === 400 && raw && typeof raw === 'object' && 'message' in raw) {
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
  }

  return err.message || 'Não foi possível redefinir a senha.'
}

export function ResetPasswordPage() {
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()

  const queryResult = useMemo(
    () =>
      resetPasswordQuerySchema.safeParse({
        userId: searchParams.get('userId'),
        token: searchParams.get('token'),
      }),
    [searchParams],
  )

  const linkErrorMessage = useMemo(() => {
    if (queryResult.success) {
      return null
    }

    return queryResult.error.issues.map((i) => i.message).join(' ')
  }, [queryResult])

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ResetPasswordFormValues>({
    resolver: zodResolver(resetPasswordFormSchema),
    defaultValues: { password: '', confirmPassword: '' },
  })

  const mutation = useMutation({
    mutationFn: (values: ResetPasswordFormValues) =>
      resetPassword({
        userId: queryResult.success ? queryResult.data.userId : '',
        token: queryResult.success ? queryResult.data.token : '',
        password: values.password,
        confirmPassword: values.confirmPassword,
      }),
    onSuccess: () => {
      toast.success('Senha atualizada. Faça login com a nova senha.')
      navigate('/login', { replace: true })
    },
    onError: (err) => {
      toast.error(extractResetError(err))
    },
  })

  const canSubmit = queryResult.success === true && !mutation.isPending

  return (
    <section className="mx-auto flex min-h-dvh max-w-md flex-col justify-center gap-8 px-6 py-12">
      <header className="flex flex-col gap-2 text-center">
        <h1 className="text-2xl font-semibold tracking-tight">Redefinir senha</h1>
        <p className="text-muted-foreground text-sm">
          Defina uma nova senha forte para sua conta.
        </p>
      </header>

      {linkErrorMessage ? (
        <div
          className="bg-destructive/10 border-destructive text-destructive rounded-md border p-3 text-sm"
          role="alert"
        >
          <p>{linkErrorMessage}</p>
          <p className="text-muted-foreground mt-2 text-xs">
            Peça uma nova recuperação em{' '}
            <Link
              to="/esqueci-senha"
              className="text-primary underline-offset-4 hover:underline"
            >
              Esqueci minha senha
            </Link>
            .
          </p>
        </div>
      ) : null}

      <form
        className="flex flex-col gap-4"
        noValidate
        onSubmit={handleSubmit((values) => mutation.mutate(values))}
      >
        <div className="flex flex-col gap-2">
          <Label htmlFor="reset-password">Nova senha</Label>
          <Input
            id="reset-password"
            type="password"
            autoComplete="new-password"
            disabled={!queryResult.success}
            aria-invalid={Boolean(errors.password)}
            aria-describedby={errors.password ? 'reset-password-error' : undefined}
            {...register('password')}
          />
          {errors.password ? (
            <p
              id="reset-password-error"
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
          <Label htmlFor="reset-confirm">Confirmar nova senha</Label>
          <Input
            id="reset-confirm"
            type="password"
            autoComplete="new-password"
            disabled={!queryResult.success}
            aria-invalid={Boolean(errors.confirmPassword)}
            aria-describedby={
              errors.confirmPassword ? 'reset-confirm-error' : undefined
            }
            {...register('confirmPassword')}
          />
          {errors.confirmPassword ? (
            <p
              id="reset-confirm-error"
              className="text-destructive text-sm"
              role="alert"
            >
              {errors.confirmPassword.message}
            </p>
          ) : null}
        </div>

        <Button type="submit" className="w-full" disabled={!canSubmit}>
          {mutation.isPending ? 'Salvando…' : 'Salvar nova senha'}
        </Button>
      </form>

      <p className="text-center text-sm">
        <Link
          to="/login"
          className="text-muted-foreground hover:text-foreground underline-offset-4 hover:underline"
        >
          Voltar ao login
        </Link>
      </p>
    </section>
  )
}
