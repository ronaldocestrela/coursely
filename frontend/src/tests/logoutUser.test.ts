import type { AxiosResponse } from 'axios'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { logoutUser } from '@/services/auth'
import { bareHttp } from '@/services/http'

describe('logoutUser', () => {
  afterEach(() => vi.restoreAllMocks())

  it('POST /api/auth/logout with refresh token (no Bearer interceptor recursion)', async () => {
    const postSpy = vi.spyOn(bareHttp, 'post').mockResolvedValue({
      status: 204,
      statusText: 'No Content',
      data: '',
      headers: {},
      config: {} as AxiosResponse['config'],
    } satisfies Partial<AxiosResponse> as AxiosResponse)

    await logoutUser('stored-refresh-value')

    expect(postSpy).toHaveBeenCalledWith('/api/auth/logout', {
      refreshToken: 'stored-refresh-value',
    })
  })
})
