import { z } from 'zod'

import { minimumPasswordLength } from '@/features/auth/registerSchema'

/** Same rules as registration (mirror `RegisterUserCommandValidator`). */
export const resetPasswordFormSchema = z
  .object({
    password: z
      .string()
      .min(minimumPasswordLength, `Mínimo de ${minimumPasswordLength} caracteres.`)
      .regex(/[A-Z]/, 'Inclua pelo menos uma letra maiúscula.')
      .regex(/[a-z]/, 'Inclua pelo menos uma letra minúscula.')
      .regex(/[0-9]/, 'Inclua pelo menos um número.')
      .regex(/[\W_]/, 'Inclua pelo menos um caractere especial.'),
    confirmPassword: z.string(),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: 'As senhas não conferem.',
    path: ['confirmPassword'],
  })

export type ResetPasswordFormValues = z.infer<typeof resetPasswordFormSchema>

export const resetPasswordQuerySchema = z.object({
  userId: z
    .string()
    .uuid('Link inválido: identificador do usuário ausente ou inválido.'),
  token: z.string().min(1, 'Link inválido: token ausente.'),
})
