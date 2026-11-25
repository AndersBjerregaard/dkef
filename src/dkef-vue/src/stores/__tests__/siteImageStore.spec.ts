import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { AxiosHeaders, type AxiosResponse } from 'axios'
import apiservice from '@/services/apiservice'
import { getSiteImageSlot } from '@/content/siteImages'
import { useSiteImageStore } from '../siteImageStore'

vi.mock('@/services/apiservice', () => ({
  default: { get: vi.fn(), put: vi.fn(), delete: vi.fn() },
}))
vi.mock('@/services/urlservice', () => ({
  default: {
    getSiteImages: (group: string) => `/SiteImages/${group}`,
    siteImage: (group: string, slot: string) => `/SiteImages/${group}/${slot}`,
  },
}))

const get = vi.mocked(apiservice.get)
const put = vi.mocked(apiservice.put)
const del = vi.mocked(apiservice.delete)
const replacement = { url: 'https://images.example.dk/new.webp', alt: 'Ny elmast' }
const file = new File(['image'], 'mast.png', { type: 'image/png' })
const response = (data: unknown): AxiosResponse => ({
  data,
  status: 200,
  statusText: 'OK',
  headers: new AxiosHeaders(),
  config: { headers: new AxiosHeaders() },
})
const groupResponse = (overrides = {}) => response({ group: 'home', overrides })

beforeEach(() => {
  vi.resetAllMocks()
  setActivePinia(createPinia())
})

describe('site image state', () => {
  it('uses defaults immediately, keeps them on failures and retries failed reads', async () => {
    const store = useSiteImageStore()
    expect(store.value('home', 'hero')).toEqual(getSiteImageSlot('home', 'hero'))
    get.mockRejectedValueOnce(new Error('offline'))
    await expect(store.loadGroup('home')).rejects.toThrow('offline')
    expect(store.value('home', 'hero').url).toBe(getSiteImageSlot('home', 'hero').url)
    get.mockResolvedValueOnce(groupResponse({ hero: replacement }))
    await store.loadGroup('home')
    expect(store.value('home', 'hero')).toEqual(replacement)
    expect(get).toHaveBeenLastCalledWith('/SiteImages/home', {
      skipAuth: true,
      withCredentials: false,
    })
  })

  it('deduplicates page consumers but refreshes whenever the editor opens', async () => {
    const store = useSiteImageStore()
    get.mockResolvedValue(groupResponse())
    await Promise.all([store.loadGroup('home'), store.loadGroup('home')])
    await store.loadGroup('home')
    expect(get).toHaveBeenCalledTimes(1)
    await store.loadGroup('home', true)
    expect(get).toHaveBeenCalledTimes(2)
  })

  it('publishes the canonical response only after a successful multipart upload', async () => {
    const store = useSiteImageStore()
    put.mockResolvedValue(response(replacement))
    await store.change('home', 'hero', file, '  Ny elmast  ')
    const [url, form, config] = put.mock.calls[0]
    expect(url).toBe('/SiteImages/home/hero')
    expect((form as FormData).get('file')).toBe(file)
    expect((form as FormData).get('alt')).toBe('  Ny elmast  ')
    expect(config).toEqual({ timeout: 60000 })
    expect(store.value('home', 'hero')).toEqual(replacement)
    expect(store.saving.home).toBe(false)
  })

  it('preserves the published image on failed saves or resets', async () => {
    const store = useSiteImageStore()
    get.mockResolvedValue(groupResponse({ hero: replacement }))
    await store.loadGroup('home')
    put.mockRejectedValue(new Error('failed upload'))
    await expect(store.change('home', 'hero', file, 'Ny')).rejects.toThrow()
    del.mockRejectedValue(new Error('failed reset'))
    await expect(store.change('home', 'hero')).rejects.toThrow()
    expect(store.value('home', 'hero')).toEqual(replacement)
    expect(store.saving.home).toBe(false)
  })

  it('resets just the selected slot after the API confirms deletion', async () => {
    const store = useSiteImageStore()
    get.mockResolvedValue(groupResponse({ hero: replacement, network: replacement }))
    await store.loadGroup('home')
    del.mockResolvedValue(response(null))
    await store.change('home', 'hero')
    expect(store.value('home', 'hero').url).toBe(getSiteImageSlot('home', 'hero').url)
    expect(store.value('home', 'network')).toEqual(replacement)
  })

  it('does not let a stale public GET undo a save or reset', async () => {
    const store = useSiteImageStore()
    let resolve!: (value: AxiosResponse) => void
    get.mockReturnValue(
      new Promise((done) => {
        resolve = done
      }),
    )
    const read = store.loadGroup('home')
    await Promise.resolve()
    put.mockResolvedValue(response(replacement))
    await store.change('home', 'hero', file, 'Ny elmast')
    resolve(groupResponse())
    await read
    expect(store.value('home', 'hero')).toEqual(replacement)

    const refresh = store.loadGroup('home')
    await Promise.resolve()
    del.mockResolvedValue(response(null))
    await store.change('home', 'hero')
    resolve(groupResponse({ hero: replacement }))
    await refresh
    expect(store.value('home', 'hero').url).toBe(getSiteImageSlot('home', 'hero').url)
  })

  it.each([
    { group: 'about', overrides: {} },
    { group: 'home', overrides: [] },
    { group: 'home', overrides: { unknown: replacement } },
    { group: 'home', overrides: { hero: { url: 'javascript:alert(1)', alt: 'Ny' } } },
  ])('rejects invalid API responses: %j', async (data) => {
    get.mockResolvedValue(response(data))
    const store = useSiteImageStore()
    await expect(store.loadGroup('home')).rejects.toThrow()
    expect(store.value('home', 'hero').url).toBe(getSiteImageSlot('home', 'hero').url)
  })

  it('never persists overrides into a new app instance', async () => {
    put.mockResolvedValue(response(replacement))
    await useSiteImageStore().change('home', 'hero', file, 'Ny elmast')
    setActivePinia(createPinia())
    expect(useSiteImageStore().value('home', 'hero').url).toBe(getSiteImageSlot('home', 'hero').url)
  })
})
