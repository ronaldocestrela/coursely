import * as React from 'react'

import type { CourseVisibility } from '@/types/courseApi'
import { cn } from '@/lib/utils'

const labels: Record<CourseVisibility, string> = {
  Private: 'Privado',
  Public: 'Público',
  Shared: 'Compartilhado',
}

const accents: Record<CourseVisibility, string> = {
  Private: 'border-border bg-secondary/80 text-secondary-foreground',
  Public: 'border-primary/30 bg-primary/10 text-primary dark:bg-primary/15',
  Shared: 'border-amber-500/35 bg-amber-500/10 text-amber-900 dark:border-amber-400/30 dark:bg-amber-400/10 dark:text-amber-100',
}

export type CourseVisibilityLabelProps = Omit<React.ComponentProps<'span'>, 'children'> & {
  visibility: CourseVisibility
}

/** Rótulo curto em PT-BR para o nível de visibilidade do curso. */
export function CourseVisibilityLabel({
  visibility,
  className,
  ...props
}: CourseVisibilityLabelProps) {
  return (
    <span
      data-slot="course-visibility-label"
      data-visibility={visibility}
      className={cn(
        'inline-flex shrink-0 items-center rounded-md border px-2 py-0.5 text-xs font-medium',
        accents[visibility],
        className,
      )}
      {...props}
    >
      {labels[visibility]}
    </span>
  )
}
