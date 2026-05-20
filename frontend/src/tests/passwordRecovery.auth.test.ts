import type { AxiosResponse } from 'axios'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { forgotPassword, resetPassword } from '@/services/auth'
import { http } from '@/services/http'

describe('forgotPassword / resetPassword', () => {
  afterEach(() => vi.restoreAllMocks())

  it('calls POST /api/auth/forgot-password with email', async () => {
    const spy = vi.spyOn(http, 'post').mockResolvedValue({
      status: 200,
      statusText: 'OK',
      data: {
        message:
          'Se este e-mail estiver cadastrado, enviaremos instruções para redefinir a senha em instantes.',
      },
      headers: {},
      config: {} as AxiosResponse['config'],
    } satisfies Partial<AxiosResponse>)

    await expect(
      forgotPassword({ email: 'recover@example.com' }),
    ).resolves.toMatchObject({
      message:
        'Se este e-mail estiver cadastrado, enviaremos instruções para redefinir a senha em instantes.',
    })

    expect(spy).toHaveBeenCalledWith('/api/auth/forgot-password', {
      email: 'recover@example.com',
    })
  })

  it('calls POST /api/auth/reset-password with full payload', async () => {
    const spy = vi.spyOn(http, 'post').mockResolvedValue({
      status: 204,
      statusText: 'No Content',
      data: '',
      headers: {},
      config: {} as AxiosResponse['config'],
    } satisfies Partial<AxiosResponse>)

    await resetPassword({
      userId: 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee',
      token: 'raw-token-from-query',
      password: 'Strong1!x',
      confirmPassword: 'Strong1!x',
    })

    expect(spy).toHaveBeenCalledWith('/api/auth/reset-password', {
      userId: 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee',
      token: 'raw-token-from-query',
      password: 'Strong1!x',
      confirmPassword: 'Strong1!x',
    })
  })
})
