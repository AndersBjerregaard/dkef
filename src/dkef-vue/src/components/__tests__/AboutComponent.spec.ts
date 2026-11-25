import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { DOMWrapper, flushPromises, mount, type VueWrapper } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import { AxiosHeaders, type AxiosResponse } from 'axios'
import AboutComponent from '../AboutComponent.vue'
import ContactFormularComponent from '../ContactFormularComponent.vue'
import { getSiteTextDefaults, getSiteTextFields, getSiteTextGroup } from '@/content/siteTexts'
import { useAuthStore } from '@/stores/authStore'
import apiservice from '@/services/apiservice'

vi.mock('@/services/apiservice', () => ({
  default: { get: vi.fn(), patch: vi.fn(), post: vi.fn() },
}))
vi.mock('@/services/urlservice', () => ({
  default: {
    getSiteTexts: (group: string) => `/SiteTexts/${group}`,
    patchSiteTexts: (group: string) => `/SiteTexts/${group}`,
    postContactMessage: () => '/information',
  },
}))
vi.mock('vue-sonner', () => ({ toast: { success: vi.fn(), error: vi.fn() } }))

const get = vi.mocked(apiservice.get)
const patch = vi.mocked(apiservice.patch)
const post = vi.mocked(apiservice.post)
const definition = getSiteTextGroup('about')!
const defaults = getSiteTextDefaults(definition)
const wrappers: VueWrapper[] = []
const response = (overrides = {}): AxiosResponse => ({
  data: { group: 'about', overrides },
  status: 200,
  statusText: 'OK',
  headers: new AxiosHeaders(),
  config: { headers: new AxiosHeaders() },
})
function render() {
  const wrapper = mount(AboutComponent, {
    attachTo: document.body,
    global: { stubs: { teleport: false, transition: false } },
  })
  wrappers.push(wrapper)
  return wrapper
}
function admin() {
  const auth = useAuthStore()
  auth.accessToken = 'test-token'
  auth.user = { email: 'admin@example.test', name: 'Admin', roles: ['Admin'] }
  return auth
}
const ui = () => new DOMWrapper(document.body)
const field = (key: string) => ui().get(`[id$="-${key}"]`)

beforeEach(() => {
  vi.resetAllMocks()
  vi.stubGlobal(
    'ResizeObserver',
    class {
      observe() {}
      unobserve() {}
      disconnect() {}
    },
  )
  setActivePinia(createPinia())
  get.mockResolvedValue(response())
})
afterEach(() => {
  wrappers.splice(0).forEach((wrapper) => wrapper.unmount())
  document.body.innerHTML = ''
  vi.unstubAllGlobals()
})

describe('About editable text integration', () => {
  it.each(['empty', 'failure'])(
    'keeps every original default readable on %s loads',
    async (load) => {
      if (load === 'failure') get.mockRejectedValueOnce(new Error('offline'))
      const wrapper = render()
      // Defaults are present before the asynchronous public load too.
      for (const value of Object.values(defaults)) expect(wrapper.text()).toContain(value)
      await flushPromises()
      for (const value of Object.values(defaults)) expect(wrapper.text()).toContain(value)
      expect(get).toHaveBeenCalledExactlyOnceWith('/SiteTexts/about', {
        skipAuth: true,
        withCredentials: false,
      })
      expect(wrapper.findComponent(ContactFormularComponent).exists()).toBe(true)
      expect(wrapper.find('button').text()).toBe('Send besked')
    },
  )

  it('binds every approved slot independently and leaves facts, images and form unchanged', async () => {
    const overrides = Object.fromEntries(
      getSiteTextFields(definition).map((field) => [field.key, `Ny ${field.key}`]),
    )
    get.mockResolvedValueOnce(response(overrides))
    const wrapper = render()
    await flushPromises()
    const slots = wrapper.findAll('h1, h2, h3, p')
    expect(slots.map((slot) => slot.text())).toEqual([
      overrides['page.title'],
      overrides['page.subtitle'],
      overrides['introduction.title'],
      overrides['introduction.description'],
      overrides['sections.title'],
      overrides['statistics.members.label'],
      '+550',
      overrides['statistics.sections.label'],
      '5',
      overrides['statistics.founded.label'],
      '1903',
      overrides['secretariat.title'],
      overrides['secretariat.name'],
      overrides['secretariat.address'],
      overrides['secretariat.postal-city'],
      `Tlf. ${overrides['secretariat.phone']}`,
      `Email: ${overrides['secretariat.email']}`,
      overrides['contact.title'],
    ])
    expect(slots.map((slot) => slot.element.tagName)).toEqual([
      'H1',
      'H2',
      'H2',
      'P',
      'H2',
      'H2',
      'H1',
      'H2',
      'H1',
      'H2',
      'H1',
      'H1',
      'H3',
      'H3',
      'H3',
      'H3',
      'H3',
      'H1',
    ])
    expect(wrapper.findAll('img').map((img) => img.attributes('src'))).toEqual([
      '/src/assets/dkef-sections.png',
      '/src/assets/secretary.jpg',
    ])
    expect(wrapper.findAll('img')[1].attributes('alt')).toBe('secretary')
    expect(wrapper.findComponent(ContactFormularComponent).exists()).toBe(true)
    expect(wrapper.findAll('form')).toHaveLength(1)
    expect(wrapper.find('a[href^="mailto:"]').exists()).toBe(false)
  })

  it('renders markup literally and preserves multiline prose in its existing paragraph', async () => {
    const prose = 'Første linje\n<b>Anden linje</b>'
    get.mockResolvedValueOnce(
      response({
        'page.title': '<img src=x onerror=alert(1)>',
        'introduction.description': prose,
      }),
    )
    const wrapper = render()
    await flushPromises()
    expect(wrapper.get('h1').text()).toBe('<img src=x onerror=alert(1)>')
    expect(wrapper.get('p').element.textContent).toBe(prose)
    expect(wrapper.get('p').classes()).toContain('whitespace-pre-line')
    expect(wrapper.find('b').exists()).toBe(false)
    expect(wrapper.findAll('img')).toHaveLength(2)
  })

  it('reacts to Admin login and role loss without discarding the open draft', async () => {
    const wrapper = render()
    await flushPromises()
    expect(wrapper.text()).not.toContain('Rediger tekster')
    const auth = admin()
    auth.user!.roles = ['Member']
    await flushPromises()
    expect(wrapper.text()).not.toContain('Rediger tekster')
    auth.user!.roles = ['Admin']
    await flushPromises()
    await wrapper.get('button').trigger('click')
    await flushPromises()
    await field('page.title').setValue('Behold kladde')
    auth.clearAuth()
    await flushPromises()
    expect(wrapper.text()).not.toContain('Rediger tekster')
    expect((field('page.title').element as HTMLInputElement).value).toBe('Behold kladde')
    expect(ui().get('[role="dialog"] button[type="submit"]').attributes('disabled')).toBeDefined()
    expect(wrapper.get('h1').text()).toBe(defaults['page.title'])
  })

  it('publishes a multi-field save immediately through the shared editor', async () => {
    admin()
    const wrapper = render()
    await flushPromises()
    await wrapper.get('button').trigger('click')
    await flushPromises()
    const changes = { 'page.title': 'Ny overskrift', 'secretariat.email': 'kontor@example.test' }
    for (const [key, value] of Object.entries(changes)) await field(key).setValue(value)
    expect(wrapper.get('h1').text()).toBe(defaults['page.title'])
    patch.mockResolvedValueOnce(response(changes))
    await ui().get('[role="dialog"] form').trigger('submit')
    await flushPromises()
    expect(patch).toHaveBeenCalledExactlyOnceWith('/SiteTexts/about', { changes })
    expect(wrapper.get('h1').text()).toBe(changes['page.title'])
    expect(wrapper.text()).toContain('Email: kontor@example.test')
    await vi.waitFor(() => expect(ui().find('[role="dialog"]').exists()).toBe(false))
    expect(post).not.toHaveBeenCalled()
  })

  it('keeps contact submission independent from editable display details', async () => {
    get.mockResolvedValueOnce(
      response({
        'secretariat.email': 'display@example.test',
        'secretariat.phone': '+45 12345678',
      }),
    )
    post.mockResolvedValueOnce(response())
    const wrapper = render()
    await flushPromises()
    await wrapper.get('#name_input').setValue('Test Person')
    await wrapper.get('#phone_input').setValue('87654321')
    await wrapper.get('#email_input').setValue('sender@example.test')
    await wrapper.get('#message_input').setValue('En besked')
    await wrapper.get('form').trigger('submit')
    await flushPromises()
    expect(post).toHaveBeenCalledExactlyOnceWith(
      '/information',
      {
        name: 'Test Person',
        phone: '87654321',
        email: 'sender@example.test',
        message: 'En besked',
      },
      { skipAuth: true },
    )
    expect((wrapper.get('#email_input').element as HTMLInputElement).value).toBe('')
    expect(wrapper.text()).toContain('Email: display@example.test')
    expect(patch).not.toHaveBeenCalled()
  })
})
