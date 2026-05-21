import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import { CourseCard } from '@/components/course/CourseCard'
import type { Course } from '@/types/courseApi'

const mockCourse = (
  overrides: Partial<Course> = {},
): Course => ({
  id: '1e4f0ec6-4a3c-4c8b-9c2e-8f3a2d1b0001',
  userId: '11111111-1111-1111-1111-111111111111',
  title: 'Design Systems na prática',
  description: 'Uma descrição longa o bastante para aparecer no card.',
  purchaseLink: 'https://example.com/buy',
  thumbnailUrl: null,
  category: 'Design',
  visibility: 'Public',
  createdAt: new Date().toISOString(),
  updatedAt: new Date().toISOString(),
  ...overrides,
})

describe('CourseCard', () => {
  it('renders title, description, category and visibility', () => {
    const course = mockCourse()
    render(<CourseCard course={course} />)

    expect(screen.getByRole('heading', { level: 3, name: course.title })).toBeInTheDocument()
    expect(screen.getByText(course.description!)).toBeInTheDocument()
    expect(screen.getByText(course.category!)).toBeInTheDocument()
    expect(screen.getByText('Público')).toBeInTheDocument()
  })

  it('shows placeholder when there is no category', () => {
    const course = mockCourse({ category: null })
    render(<CourseCard course={course} />)

    expect(screen.getByText('Sem categoria')).toBeInTheDocument()
  })
})
