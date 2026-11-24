import { beforeEach, describe, expect, it, vi } from 'vitest'
import {
  AxiosError,
  AxiosHeaders,
  type AxiosResponse,
  type InternalAxiosRequestConfig,
} from 'axios'
import { flushPromises } from '@vue/test-utils'

const auth = vi.hoisted(() => ({
  accessToken: 'old-token',
  refreshAccessToken: vi.fn(),
  fetchUserProfile: vi.fn(),
  handleSessionExpired: vi.fn(),
}))
vi.mock('@/stores/authStore', () => ({ useAuthStore: () => auth }))

function success(config: InternalAxiosRequestConfig): AxiosResponse {
  return { data: { ok: true }, status: 200, statusText: 'OK', headers: new AxiosHeaders(), config }
}
function unauthorized(config: InternalAxiosRequestConfig): never {
  throw new AxiosError('Unauthorized', 'ERR_BAD_REQUEST', config, undefined, {
    ...success(config),
    status: 401,
  })
}

beforeEach(() => {
  vi.resetModules()
  vi.resetAllMocks()
  auth.accessToken = 'old-token'
})

describe('API authentication and PATCH wrapper', () => {
  it('passes PATCH body/config through and attaches Bearer authentication', async () => {
    const api = (await import('../apiservice')).default
    const adapter = vi.fn(async (config: InternalAxiosRequestConfig) => success(config))
    api.axiosInstance.defaults.adapter = adapter
    await api.patch<{ ok: boolean }>(
      '/SiteTexts/home',
      { changes: { 'hero.title': 'Ny' } },
      { timeout: 1234 },
    )
    const config = adapter.mock.calls[0][0]
    expect(config.method).toBe('patch')
    expect(JSON.parse(config.data)).toEqual({ changes: { 'hero.title': 'Ny' } })
    expect(config.timeout).toBe(1234)
    expect(config.headers.Authorization).toBe('Bearer old-token')
  })

  it('does not send credentials, refresh tokens, expire sessions, or prompt on public 401', async () => {
    const api = (await import('../apiservice')).default
    const prompt = vi.fn()
    window.addEventListener('auth:prompt-login', prompt)
    const adapter = vi.fn(async (config: InternalAxiosRequestConfig) => unauthorized(config))
    api.axiosInstance.defaults.adapter = adapter
    try {
      await expect(
        api.get('/SiteTexts/home', { skipAuth: true, withCredentials: false }),
      ).rejects.toMatchObject({ response: { status: 401 } })
      const config = adapter.mock.calls[0][0]
      expect(config.headers.Authorization).toBeUndefined()
      expect(config.withCredentials).toBe(false)
      expect(auth.refreshAccessToken).not.toHaveBeenCalled()
      expect(auth.handleSessionExpired).not.toHaveBeenCalled()
      expect(prompt).not.toHaveBeenCalled()
    } finally {
      window.removeEventListener('auth:prompt-login', prompt)
    }
  })

  it('refreshes authenticated PATCH and retries with the new token and same body', async () => {
    const api = (await import('../apiservice')).default
    auth.refreshAccessToken.mockImplementation(async () => {
      auth.accessToken = 'new-token'
      return 'new-token'
    })
    const tokens: unknown[] = []
    const bodies: unknown[] = []
    api.axiosInstance.defaults.adapter = async (config) => {
      tokens.push(config.headers.Authorization)
      bodies.push(JSON.parse(config.data))
      if (tokens.length === 1) unauthorized(config)
      return success(config)
    }
    await api.patch('/SiteTexts/home', { changes: { 'hero.title': 'Ny' } })
    expect(tokens).toEqual(['Bearer old-token', 'Bearer new-token'])
    expect(bodies).toEqual([
      { changes: { 'hero.title': 'Ny' } },
      { changes: { 'hero.title': 'Ny' } },
    ])
    expect(auth.refreshAccessToken).toHaveBeenCalledTimes(1)
    expect(auth.fetchUserProfile).toHaveBeenCalledTimes(1)
    expect(auth.handleSessionExpired).not.toHaveBeenCalled()
  })

  it('queues concurrent authenticated requests behind a single token refresh', async () => {
    const api = (await import('../apiservice')).default
    let finish!: (token: string) => void
    let started!: () => void
    const refreshStarted = new Promise<void>((resolve) => {
      started = resolve
    })
    auth.refreshAccessToken.mockImplementation(() => {
      started()
      return new Promise<string>((resolve) => {
        finish = resolve
      })
    })
    let queued!: () => void
    const secondStarted = new Promise<void>((resolve) => {
      queued = resolve
    })
    const attempts: string[] = []
    api.axiosInstance.defaults.adapter = async (config) => {
      attempts.push(`${config.url}:${config.headers.Authorization}`)
      if (config.headers.Authorization === 'Bearer old-token') {
        if (config.url === '/second') queued()
        unauthorized(config)
      }
      return success(config)
    }
    const first = api.patch('/first', {})
    await refreshStarted
    const second = api.patch('/second', {})
    await secondStarted
    // Let the response interceptor enqueue the second 401 before completing refresh.
    await flushPromises()
    expect(attempts).toHaveLength(2)
    auth.accessToken = 'new-token'
    finish('new-token')
    await Promise.all([first, second])
    expect(auth.refreshAccessToken).toHaveBeenCalledTimes(1)
    expect(attempts).toContain('/first:Bearer new-token')
    expect(attempts).toContain('/second:Bearer new-token')
  })

  it('preserves session-expired/login behavior when authenticated refresh fails', async () => {
    const api = (await import('../apiservice')).default
    auth.refreshAccessToken.mockRejectedValueOnce(new Error('expired'))
    api.axiosInstance.defaults.adapter = async (config) => unauthorized(config)
    const prompt = vi.fn()
    window.addEventListener('auth:prompt-login', prompt)
    try {
      await expect(api.patch('/SiteTexts/home', {})).rejects.toThrow('expired')
      expect(auth.handleSessionExpired).toHaveBeenCalledWith(true)
      expect(prompt).toHaveBeenCalledTimes(1)
    } finally {
      window.removeEventListener('auth:prompt-login', prompt)
    }
  })
})
