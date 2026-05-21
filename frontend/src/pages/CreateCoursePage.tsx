import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation } from '@tanstack/react-query'
import axios from 'axios'
import { useForm } from 'react-hook-form'
import { Link, useNavigate } from 'react-router-dom'
import { toast } from 'sonner'

import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import {
  createCourseFormSchema,
  type CreateCourseFormValues,
  type CreateCourseOutput,
} from '@/features/courses/courseFormSchema'
import { type CreateCourseDto, createCourse } from '@/services/courses'

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
    const message = (raw as { message?: unknown }).message
    if (typeof message === 'string') {
      return message
    }
    const title = (raw as { title?: unknown }).title
    if (typeof title === 'string') {
      return title
    }
  }

  return err.message || 'Não foi possível criar o curso.'
}

function toCreateDto(parsed: CreateCourseOutput): CreateCourseDto {
  return {
    title: parsed.title,
    description: parsed.description,
    purchaseLink: parsed.purchaseLink,
    thumbnailUrl: parsed.thumbnailUrl,
    category: parsed.category,
    visibility: parsed.visibility,
  }
}

export function CreateCoursePage() {
  const navigate = useNavigate()
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<CreateCourseFormValues, unknown, CreateCourseOutput>({
    resolver: zodResolver(createCourseFormSchema),
    defaultValues: {
      title: '',
      description: '',
      purchaseLink: '',
      thumbnailUrl: '',
      category: '',
      visibility: 'Private',
    },
  })

  const mutation = useMutation({
    mutationFn: (values: CreateCourseOutput) => createCourse(toCreateDto(values)),
    onSuccess: () => {
      toast.success('Curso adicionado à sua lista.')
      navigate('/dashboard', { replace: true })
    },
    onError: (err) => {
      toast.error(extractApiErrorMessage(err))
    },
  })

  return (
    <section className="mx-auto flex min-h-dvh max-w-lg flex-col justify-center gap-8 px-6 py-12">
      <header className="flex flex-col gap-2 text-center">
        <h1 className="text-2xl font-semibold tracking-tight">Criar curso</h1>
        <p className="text-muted-foreground text-sm">
          Adicione um curso à sua lista de desejos. Você pode editar ou remover
          depois.
        </p>
      </header>

      <form
        className="flex flex-col gap-4"
        noValidate
        onSubmit={handleSubmit((values) => mutation.mutate(values))}
      >
        <div className="flex flex-col gap-2">
          <Label htmlFor="course-title">Título</Label>
          <Input
            id="course-title"
            type="text"
            autoComplete="off"
            aria-invalid={Boolean(errors.title)}
            aria-describedby={errors.title ? 'course-title-error' : undefined}
            {...register('title')}
          />
          {errors.title ? (
            <p
              id="course-title-error"
              className="text-destructive text-sm"
              role="alert"
            >
              {errors.title.message}
            </p>
          ) : null}
        </div>

        <div className="flex flex-col gap-2">
          <Label htmlFor="course-description">Descrição</Label>
          <Textarea
            id="course-description"
            rows={4}
            aria-invalid={Boolean(errors.description)}
            aria-describedby={
              errors.description ? 'course-description-error' : undefined
            }
            {...register('description')}
          />
          {errors.description ? (
            <p
              id="course-description-error"
              className="text-destructive text-sm"
              role="alert"
            >
              {errors.description.message}
            </p>
          ) : null}
        </div>

        <div className="flex flex-col gap-2">
          <Label htmlFor="course-purchase-link">Link de compra</Label>
          <Input
            id="course-purchase-link"
            type="url"
            inputMode="url"
            placeholder="https://"
            autoComplete="off"
            aria-invalid={Boolean(errors.purchaseLink)}
            aria-describedby={
              errors.purchaseLink ? 'course-purchase-link-error' : undefined
            }
            {...register('purchaseLink')}
          />
          {errors.purchaseLink ? (
            <p
              id="course-purchase-link-error"
              className="text-destructive text-sm"
              role="alert"
            >
              {errors.purchaseLink.message}
            </p>
          ) : null}
        </div>

        <div className="flex flex-col gap-2">
          <Label htmlFor="course-thumbnail">URL da capa (opcional)</Label>
          <Input
            id="course-thumbnail"
            type="url"
            inputMode="url"
            placeholder="https://"
            autoComplete="off"
            aria-invalid={Boolean(errors.thumbnailUrl)}
            aria-describedby={
              errors.thumbnailUrl ? 'course-thumbnail-error' : undefined
            }
            {...register('thumbnailUrl')}
          />
          {errors.thumbnailUrl ? (
            <p
              id="course-thumbnail-error"
              className="text-destructive text-sm"
              role="alert"
            >
              {errors.thumbnailUrl.message}
            </p>
          ) : null}
        </div>

        <div className="flex flex-col gap-2">
          <Label htmlFor="course-category">Categoria</Label>
          <Input
            id="course-category"
            type="text"
            autoComplete="off"
            aria-invalid={Boolean(errors.category)}
            aria-describedby={errors.category ? 'course-category-error' : undefined}
            {...register('category')}
          />
          {errors.category ? (
            <p
              id="course-category-error"
              className="text-destructive text-sm"
              role="alert"
            >
              {errors.category.message}
            </p>
          ) : null}
        </div>

        <div className="flex flex-col gap-2">
          <Label htmlFor="course-visibility">Visibilidade</Label>
          <select
            id="course-visibility"
            className="border-input focus-visible:border-ring focus-visible:ring-ring/50 aria-invalid:ring-destructive/20 dark:aria-invalid:ring-destructive/40 aria-invalid:border-destructive dark:bg-input/30 h-9 w-full rounded-md border bg-transparent px-3 text-base shadow-xs outline-none focus-visible:ring-[3px] md:text-sm"
            aria-invalid={Boolean(errors.visibility)}
            aria-describedby={
              errors.visibility ? 'course-visibility-error' : undefined
            }
            {...register('visibility')}
          >
            <option value="Private">Privado</option>
            <option value="Public">Público</option>
            <option value="Shared">Compartilhado</option>
          </select>
          {errors.visibility ? (
            <p
              id="course-visibility-error"
              className="text-destructive text-sm"
              role="alert"
            >
              {errors.visibility.message}
            </p>
          ) : null}
        </div>

        <Button type="submit" className="w-full" disabled={mutation.isPending}>
          {mutation.isPending ? 'Salvando…' : 'Salvar curso'}
        </Button>
      </form>

      <p className="text-muted-foreground text-center text-sm">
        <Link
          to="/dashboard"
          className="text-primary font-medium underline-offset-4 hover:underline"
        >
          Voltar ao painel
        </Link>
      </p>
    </section>
  )
}
