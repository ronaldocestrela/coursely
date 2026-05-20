import { http } from '@/services/http'

/** Plain-text response from ASP.NET Core health endpoint. */
export async function fetchApiHealth(): Promise<string> {
  const response = await http.get<string>('/health', {
    responseType: 'text',
  })

  return typeof response.data === 'string' ? response.data : String(response.data)
}
