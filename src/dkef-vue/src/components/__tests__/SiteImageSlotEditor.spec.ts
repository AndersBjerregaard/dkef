import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises, mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import { AxiosHeaders, type AxiosResponse } from 'axios'
import SiteImageSlotEditor from '../SiteImageSlotEditor.vue'
import { getSiteImageSlot } from '@/content/siteImages'
import { useAuthStore } from '@/stores/authStore'
import { useSiteImageStore } from '@/stores/siteImageStore'
import apiservice from '@/services/apiservice'

vi.mock('@/services/apiservice', () => ({
  default: { get: vi.fn(), put: vi.fn(), delete: vi.fn() },
}))
vi.mock('@/services/urlservice', () => ({ default: { siteImage: () => '/SiteImages/home/hero' } }))
vi.mock('vue-sonner', () => ({ toast: { success: vi.fn() } }))

function response(data: unknown): AxiosResponse {
  return {
    data,
    status: 200,
    statusText: 'OK',
    headers: new AxiosHeaders(),
    config: { headers: new AxiosHeaders() },
  }
}

beforeEach(() => {
  vi.resetAllMocks()
  setActivePinia(createPinia())
  const auth = useAuthStore()
  auth.accessToken = 'test-token'
  auth.user = { email: 'admin@example.dk', name: 'Admin', roles: ['Admin'] }
  vi.stubGlobal(
    'URL',
    class extends URL {
      static createObjectURL = vi.fn(() => 'blob:preview')
      static revokeObjectURL = vi.fn()
    },
  )
})

afterEach(() => vi.unstubAllGlobals())

describe('image editor draft', () => {
  it('provides a labelled, visibly styled file picker and disables it while saving', async () => {
    const wrapper = mount(SiteImageSlotEditor, {
      props: { group: 'home', slotDefinition: getSiteImageSlot('home', 'hero') },
    })
    const picker = wrapper.get<HTMLInputElement>('input[type=file]')
    expect(wrapper.get('label').attributes('for')).toBe(picker.attributes('id'))
    expect(picker.classes()).toEqual(
      expect.arrayContaining(['cursor-pointer', 'border', 'bg-theme-soft', 'file:cursor-pointer']),
    )
    expect(picker.element.disabled).toBe(false)
    let finishSave!: (value: AxiosResponse) => void
    vi.mocked(apiservice.delete).mockReturnValueOnce(
      new Promise<AxiosResponse>((resolve) => {
        finishSave = resolve
      }),
    )
    const saving = useSiteImageStore().change('home', 'hero')
    await flushPromises()
    expect(picker.element.disabled).toBe(true)
    expect(picker.classes()).toContain('disabled:cursor-not-allowed')
    finishSave(response(null))
    await saving
    useAuthStore().user!.roles = ['Member']
    await flushPromises()
    expect(picker.element.disabled).toBe(true)
    wrapper.unmount()
  })

  it('does not upload until save; keeps a failed draft and releases the preview after success', async () => {
    const wrapper = mount(SiteImageSlotEditor, {
      props: { group: 'home', slotDefinition: getSiteImageSlot('home', 'hero') },
    })
    const file = new File(['image'], 'new.png', { type: 'image/png' })
    Object.defineProperty(wrapper.get('input[type=file]').element, 'files', {
      value: [file],
      configurable: true,
    })
    await wrapper.get('input[type=file]').trigger('change')
    expect(wrapper.text()).toContain('endnu ikke gemt')
    expect(wrapper.get('img').attributes('src')).toBe('blob:preview')
    expect(apiservice.put).not.toHaveBeenCalled()
    await wrapper.get('input[type=text]').setValue('Ny elmast')
    vi.mocked(apiservice.put).mockRejectedValueOnce(new Error('offline'))
    await wrapper.get('form').trigger('submit')
    await flushPromises()
    expect(wrapper.text()).toContain('Dit valgte billede er bevaret')
    expect(wrapper.get<HTMLInputElement>('input[type=text]').element.value).toBe('Ny elmast')
    vi.mocked(apiservice.put).mockResolvedValueOnce(
      response({ url: 'https://images.example.dk/new.webp', alt: 'Ny elmast' }),
    )
    await wrapper.get('form').trigger('submit')
    await flushPromises()
    expect(wrapper.find('form').exists()).toBe(false)
    expect(wrapper.get('img').attributes('src')).toBe('https://images.example.dk/new.webp')
    expect(URL.revokeObjectURL).toHaveBeenCalledWith('blob:preview')
    wrapper.unmount()
  })

  it('rejects unsupported files locally without an upload', async () => {
    const wrapper = mount(SiteImageSlotEditor, {
      props: { group: 'home', slotDefinition: getSiteImageSlot('home', 'hero') },
    })
    Object.defineProperty(wrapper.get('input[type=file]').element, 'files', {
      value: [new File(['svg'], 'bad.svg', { type: 'image/svg+xml' })],
      configurable: true,
    })
    await wrapper.get('input[type=file]').trigger('change')
    expect(wrapper.get('[role=alert]').text()).toContain('JPEG-, PNG- eller WebP')
    expect(wrapper.find('form').exists()).toBe(false)
    expect(apiservice.put).not.toHaveBeenCalled()
    wrapper.unmount()
  })

  it('blocks saving after loss of the admin role without discarding the draft', async () => {
    const wrapper = mount(SiteImageSlotEditor, {
      props: { group: 'home', slotDefinition: getSiteImageSlot('home', 'hero') },
    })
    Object.defineProperty(wrapper.get('input[type=file]').element, 'files', {
      value: [new File(['image'], 'new.png', { type: 'image/png' })],
      configurable: true,
    })
    await wrapper.get('input[type=file]').trigger('change')
    useAuthStore().user!.roles = ['Member']
    await wrapper.get('form').trigger('submit')
    expect(apiservice.put).not.toHaveBeenCalled()
    expect(wrapper.find('form').exists()).toBe(true)
    wrapper.unmount()
  })
})
