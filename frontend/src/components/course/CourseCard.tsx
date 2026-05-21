import { BookOpen, ImageIcon } from 'lucide-react'
import * as React from 'react'

import type { Course } from '@/types/courseApi'
import { cn } from '@/lib/utils'

import { CourseBadge } from './CourseBadge'
import { CourseVisibilityLabel } from './CourseVisibilityLabel'

export type CourseCardProps = Omit<React.ComponentProps<'article'>, 'children'> & {
  course: Course
  /** Texto exibido quando não há descrição (ex.: lista compacta). */
  emptyDescriptionFallback?: string
}

/** Cartão reutilizável para exibir um curso desejado na UI. */
export function CourseCard({
  course,
  className,
  emptyDescriptionFallback = 'Sem descrição',
  ...props
}: CourseCardProps) {
  const description =
    course.description?.trim() ||
    (course.category ? null : emptyDescriptionFallback)

  return (
    <article
      data-slot="course-card"
      data-course-id={course.id}
      className={cn(
        'flex gap-4 rounded-xl border border-border bg-card p-4 text-card-foreground shadow-xs',
        className,
      )}
      {...props}
    >
      <div
        className={cn(
          'group relative size-20 shrink-0 overflow-hidden rounded-lg border border-border bg-muted sm:size-24',
          course.thumbnailUrl && 'border-0',
        )}
        aria-hidden={!course.thumbnailUrl}
      >
        {course.thumbnailUrl ? (
          <img
            src={course.thumbnailUrl}
            alt=""
            className="size-full object-cover"
            loading="lazy"
          />
        ) : (
          <div className="flex size-full flex-col items-center justify-center gap-1 text-muted-foreground">
            <ImageIcon className="size-8 opacity-70" aria-hidden />
            <span className="sr-only">Sem imagem de capa</span>
          </div>
        )}
      </div>

      <div className="flex min-w-0 flex-1 flex-col gap-2">
        <div className="flex flex-wrap items-start justify-between gap-2">
          <div className="min-w-0 flex-1">
            <h3 className="truncate font-semibold leading-snug tracking-tight">{course.title}</h3>
            {description ? (
              <p className="mt-1 line-clamp-2 text-sm text-muted-foreground">{description}</p>
            ) : null}
          </div>
          <CourseVisibilityLabel visibility={course.visibility} />
        </div>

        <div className="flex flex-wrap items-center gap-2">
          {course.category ? (
            <CourseBadge variant="category">
              <BookOpen className="mr-1.5 size-3.5" aria-hidden />
              <span className="truncate">{course.category}</span>
            </CourseBadge>
          ) : (
            <CourseBadge variant="outline">Sem categoria</CourseBadge>
          )}
        </div>
      </div>
    </article>
  )
}
