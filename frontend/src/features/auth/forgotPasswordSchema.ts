import { z } from 'zod'

export const forgotPasswordFormSchema = z.object({
  email: z
    .string()
    .trim()
    .min(1, 'E-mail é obrigatório.')
    .email('E-mail inválido.')
    .max(256, 'E-mail muito longo.'),
})

export type ForgotPasswordFormValues = z.infer<typeof forgotPasswordFormSchema>
