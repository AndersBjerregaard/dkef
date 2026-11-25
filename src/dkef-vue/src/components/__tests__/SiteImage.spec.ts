import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises, mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import { AxiosHeaders } from 'axios'
import SiteImage from '../SiteImage.vue'
import apiservice from '@/services/apiservice'
import { getSiteImageSlot } from '@/content/siteImages'
import { useSiteImageStore } from '@/stores/siteImageStore'

vi.mock('@/services/apiservice', () => ({ default: { get: vi.fn() } }))
vi.mock('@/services/urlservice', () => ({
  default: { getSiteImages: (group: string) => `/SiteImages/${group}` },
}))

beforeEach(() => {
  setActivePinia(createPinia())
  vi.resetAllMocks()
})

describe('site image display', () => {
  it('falls back to the bundled source and description when an override fails to load', async () => {
    vi.mocked(apiservice.get).mockResolvedValue({
      data: {
        group: 'home',
        overrides: { hero: { url: 'https://images.example.dk/missing.webp', alt: 'Ny elmast' } },
      },
      status: 200,
      statusText: 'OK',
      headers: new AxiosHeaders(),
      config: { headers: new AxiosHeaders() },
    })
    const wrapper = mount(SiteImage, {
      props: { group: 'home', imageSlot: 'hero' },
      attrs: { class: 'rounded-xl' },
    })
    await flushPromises()
    expect(wrapper.attributes('src')).toBe('https://images.example.dk/missing.webp')
    expect(wrapper.attributes('alt')).toBe('Ny elmast')
    expect(wrapper.classes()).toContain('rounded-xl')
    await wrapper.trigger('error')
    expect(wrapper.attributes('src')).toBe(getSiteImageSlot('home', 'hero').url)
    expect(wrapper.attributes('alt')).toBe(getSiteImageSlot('home', 'hero').alt)
    expect(wrapper.attributes('style')).toContain('aspect-ratio: 2 / 3')
    wrapper.unmount()
  })

  it('shows bundled images while the public API is unavailable', async () => {
    vi.mocked(apiservice.get).mockRejectedValue(new Error('offline'))
    const wrapper = mount(SiteImage, { props: { group: 'about', imageSlot: 'sections' } })
    await flushPromises()
    expect(wrapper.attributes('src')).toBe(getSiteImageSlot('about', 'sections').url)
    expect(useSiteImageStore().overrides.about).toBeUndefined()
    wrapper.unmount()
  })
})
