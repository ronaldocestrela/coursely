/**
 * Contrato alinhado ao domínio `CourseWishlist` (.NET).
 * No banco, `visibility` é int (0–2). Quando a API existir, pode serializar enum como número;
 * use um mapper se passar a enviar strings PascalCase.
 */
export type CourseVisibility = 'Private' | 'Public' | 'Shared'

export type Course = {
  id: string
  userId: string
  title: string
  description: string | null
  purchaseLink: string | null
  thumbnailUrl: string | null
  category: string | null
  visibility: CourseVisibility
  createdAt: string
  updatedAt: string
}
