import { http } from '@/services/http'
import type { Course, CourseVisibility } from '@/types/courseApi'

export type CreateCourseDto = {
  title: string
  description?: string
  purchaseLink?: string
  thumbnailUrl?: string
  category?: string
  visibility: CourseVisibility
}

export async function createCourse(body: CreateCourseDto): Promise<Course> {
  const { data } = await http.post<Course>('/api/courses', {
    title: body.title,
    description: body.description ?? null,
    purchaseLink: body.purchaseLink ?? null,
    thumbnailUrl: body.thumbnailUrl ?? null,
    category: body.category ?? null,
    visibility: body.visibility,
  })
  return data
}
