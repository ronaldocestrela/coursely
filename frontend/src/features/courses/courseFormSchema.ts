import { z } from 'zod'

const visibilityEnum = z.enum(['Private', 'Public', 'Shared'])

function optionalHttpUrl(message: string) {
  return z
    .string()
    .trim()
    .transform((s) => (s === '' ? undefined : s))
    .refine(
      (v) => {
        if (v === undefined) return true
        try {
          const u = new URL(v)
          return u.protocol === 'http:' || u.protocol === 'https:'
        } catch {
          return false
        }
      },
      { message },
    )
}

export const createCourseFormSchema = z.object({
  title: z
    .string()
    .trim()
    .min(1, 'Título é obrigatório.')
    .max(300, 'Título muito longo.'),
  description: z
    .string()
    .trim()
    .max(4000, 'Descrição muito longa.')
    .transform((s) => (s === '' ? undefined : s)),
  purchaseLink: optionalHttpUrl(
    'Link de compra deve ser uma URL http ou https válida.',
  ),
  thumbnailUrl: optionalHttpUrl(
    'URL da capa deve ser uma URL http ou https válida.',
  ),
  category: z
    .string()
    .trim()
    .max(200, 'Categoria muito longa.')
    .transform((s) => (s === '' ? undefined : s)),
  visibility: visibilityEnum,
})

export type CreateCourseFormValues = z.input<typeof createCourseFormSchema>

export type CreateCourseOutput = z.output<typeof createCourseFormSchema>
