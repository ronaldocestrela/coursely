import * as React from 'react'
import { cva, type VariantProps } from 'class-variance-authority'

import { cn } from '@/lib/utils'

const courseBadgeVariants = cva(
  'inline-flex max-w-full min-w-0 items-center rounded-md border px-2 py-0.5 text-xs font-medium',
  {
    variants: {
      variant: {
        default:
          'border-border bg-muted text-foreground dark:bg-muted/40',
        category:
          'border-primary/25 bg-primary/10 text-primary dark:bg-primary/15',
        outline: 'border-border bg-transparent text-muted-foreground',
      },
    },
    defaultVariants: {
      variant: 'category',
    },
  },
)

export interface CourseBadgeProps
  extends Omit<React.ComponentProps<'span'>, 'children' | 'title'>,
    VariantProps<typeof courseBadgeVariants> {
  children: React.ReactNode
}

export function CourseBadge({ className, variant, children, ...props }: CourseBadgeProps) {
  return (
    <span
      data-slot="course-badge"
      className={cn(courseBadgeVariants({ variant }), className)}
      {...props}
    >
      {children}
    </span>
  )
}
