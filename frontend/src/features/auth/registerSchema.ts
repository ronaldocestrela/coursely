import { z } from 'zod'

/** Matches backend `RegisterUserCommandValidator.MinimumPasswordLength` (8). */
export const minimumPasswordLength = 8

/**
 * Client-side mirror of backend password rules:
 * min 8 chars, uppercase, lowercase, digit, special character.
 */
export const registerFormSchema = z
  .object({
    name: z
      .string()
      .trim()
      .min(1, 'Nome é obrigatório.')
      .max(200, 'Nome muito longo.'),
    email: z
      .string()
      .trim()
      .min(1, 'E-mail é obrigatório.')
      .email('E-mail inválido.')
      .max(256, 'E-mail muito longo.'),
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

export type RegisterFormValues = z.infer<typeof registerFormSchema>
