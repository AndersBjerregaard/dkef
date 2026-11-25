import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { DOMWrapper, flushPromises, mount, type VueWrapper } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import { AxiosHeaders, type AxiosResponse } from 'axios'
import HomeComponent from '../HomeComponent.vue'
import MigrationBanner from '../MigrationBanner.vue'
import { getSiteTextDefaults, getSiteTextFields, getSiteTextGroup } from '@/content/siteTexts'
import { useAuthStore } from '@/stores/authStore'
import apiservice from '@/services/apiservice'
import wexoe from '@/assets/wexoe.svg'
import lauritz from '@/assets/lauritz.svg'

const { push } = vi.hoisted(() => ({ push: vi.fn() }))
vi.mock('vue-router', () => ({ useRouter: () => ({ push }) }))
vi.mock('@/services/apiservice', () => ({ default: { get: vi.fn(), patch: vi.fn() } }))
vi.mock('@/services/urlservice', () => ({
  default: {
    getSiteTexts: (group: string) => `/SiteTexts/${group}`,
    patchSiteTexts: (group: string) => `/SiteTexts/${group}`,
  },
}))
vi.mock('vue-sonner', () => ({ toast: { success: vi.fn(), error: vi.fn() } }))

const get = vi.mocked(apiservice.get)
const patch = vi.mocked(apiservice.patch)
const groups = ['home', 'membership'] as const
const defaults = Object.fromEntries(
  groups.map((group) => [group, getSiteTextDefaults(getSiteTextGroup(group)!)]),
)
const benefitKeys = [
  'benefits.development',
  'benefits.network',
  'benefits.mentoring',
  'benefits.events',
  'benefits.magazine',
]
const labels = {
  home: 'Rediger forsidens tekster',
  membership: 'Rediger fælles medlemsfordele',
}
const wrappers: VueWrapper[] = []
const response = (group: string, overrides = {}): AxiosResponse => ({
  data: { group, overrides },
  status: 200,
  statusText: 'OK',
  headers: new AxiosHeaders(),
  config: { headers: new AxiosHeaders() },
})
function render() {
  const wrapper = mount(HomeComponent, {
    attachTo: document.body,
    global: { stubs: { teleport: false, transition: false, RouterLink: true } },
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
const action = (wrapper: VueWrapper, group: (typeof groups)[number]) =>
  wrapper.findAll('button').find((button) => button.text() === labels[group])!
async function open(wrapper: VueWrapper, group: (typeof groups)[number]) {
  await action(wrapper, group).trigger('click')
  await flushPromises()
}
async function cancel() {
  await ui()
    .findAll('button')
    .find((button) => button.text() === 'Annuller')!
    .trigger('click')
  await flushPromises()
  await vi.waitFor(() => expect(ui().find('[role="dialog"]').exists()).toBe(false))
}

beforeEach(() => {
  vi.resetAllMocks()
  vi.stubGlobal('scrollTo', vi.fn())
  vi.stubGlobal('localStorage', { getItem: vi.fn(), setItem: vi.fn() })
  vi.stubGlobal(
    'ResizeObserver',
    class {
      observe() {}
      unobserve() {}
      disconnect() {}
    },
  )
  setActivePinia(createPinia())
  get.mockImplementation(async (url) => response(String(url).split('/').pop()!))
})
afterEach(() => {
  wrappers.splice(0).forEach((wrapper) => wrapper.unmount())
  document.body.innerHTML = ''
  vi.unstubAllGlobals()
})

describe('Home editable text integration', () => {
  it.each(['empty', 'failure'])('keeps all original defaults on %s loads', async (load) => {
    if (load === 'failure') get.mockRejectedValue(new Error('offline'))
    const wrapper = render()
    for (const values of Object.values(defaults))
      for (const value of Object.values(values)) expect(wrapper.text()).toContain(value)
    await flushPromises()
    for (const values of Object.values(defaults))
      for (const value of Object.values(values)) expect(wrapper.text()).toContain(value)
    for (const group of groups)
      expect(get).toHaveBeenCalledWith(`/SiteTexts/${group}`, {
        skipAuth: true,
        withCredentials: false,
      })
    expect(get).toHaveBeenCalledTimes(2)
  })

  it('binds all 32 slots independently in fixed element/order and keeps exclusions unchanged', async () => {
    const overrides = Object.fromEntries(
      groups.map((group) => [
        group,
        Object.fromEntries(
          getSiteTextFields(getSiteTextGroup(group)!).map((field) => [
            field.key,
            `Ny ${field.key}`,
          ]),
        ),
      ]),
    )
    get.mockImplementation(async (url) => {
      const group = String(url).split('/').pop()!
      return response(group, overrides[group])
    })
    const wrapper = render()
    await flushPromises()
    const slots = wrapper.get('.home-text').findAll('h1, h2, h3, p, button')
    expect(slots.map((slot) => slot.text())).toEqual([
      ...['hero.eyebrow', 'hero.title', 'hero.description'].map((key) => overrides.home[key]),
      '⭐ 4.8/5 stjerner af vores 100+ anmeldelser.',
      ...[
        'companies.title',
        'network.eyebrow',
        'network.title',
        'network.description',
        'network.action-label',
        'development.eyebrow',
        'development.title',
        'development.description',
        'development.action-label',
        'reasons.eyebrow',
        'reasons.title',
        'reasons.description',
        'reasons.action-label',
        'reasons.sparring.title',
        'reasons.sparring.description',
        'reasons.network.title',
        'reasons.network.description',
        'reasons.development.title',
        'reasons.development.description',
        'plans.title',
        'plan.title',
      ].map((key) => overrides.home[key]),
      '700,-',
      ...['plan.description', 'plan.activities-description', 'plan.action-label'].map(
        (key) => overrides.home[key],
      ),
      ...benefitKeys.map((key) => `✅ ${overrides.membership[key]}`),
    ])
    expect(slots.map((slot) => slot.element.tagName)).toEqual([
      'P',
      'H1',
      'P',
      'P',
      'H2',
      'P',
      'H2',
      'P',
      'BUTTON',
      'P',
      'H2',
      'P',
      'BUTTON',
      'P',
      'H2',
      'P',
      'BUTTON',
      'H3',
      'P',
      'H3',
      'P',
      'H3',
      'P',
      'H2',
      'H3',
      'P',
      'P',
      'P',
      'BUTTON',
      'P',
      'P',
      'P',
      'P',
      'P',
    ])
    expect(wrapper.text()).toContain('inkl. moms pr. år')
    expect(wrapper.findComponent(MigrationBanner).exists()).toBe(true)
    expect(wrapper.findAll('img').map((img) => img.attributes('alt'))).toEqual([
      'Elmast',
      'CUBIC',
      'LEDVANCE',
      'WEXOE',
      'Schneider',
      'Lauritz Knudsen',
      'Solar',
      'Mand, der studerer',
      'Kvinde på arbejde',
    ])
    expect(wrapper.findAll('img').map((img) => img.attributes('src'))).toEqual([
      '/src/assets/el-mast.jpeg',
      '/src/assets/cubic.png',
      '/src/assets/ledvance.png',
      wexoe,
      '/src/assets/schneider.png',
      lauritz,
      '/src/assets/solar.png',
      '/src/assets/man-studying.png',
      '/src/assets/corporate-woman.png',
    ])
    expect(wrapper.find('input[type="email"]').exists()).toBe(false)
  })

  it.each(groups)('loads %s even when the other group fails', async (group) => {
    const key = group === 'home' ? 'hero.title' : 'benefits.network'
    get.mockImplementation(async (url) => {
      if (url !== `/SiteTexts/${group}`) throw new Error('offline')
      return response(group, { [key]: 'Indlæst tekst' })
    })
    const wrapper = render()
    await flushPromises()
    expect(wrapper.text()).toContain('Indlæst tekst')
    const other = group === 'home' ? 'membership' : 'home'
    for (const value of Object.values(defaults[other])) expect(wrapper.text()).toContain(value)
  })

  it('renders markup literally and preserves every multiline slot', async () => {
    const fields = getSiteTextFields(getSiteTextGroup('home')!).filter(
      (field) => field.input === 'multiline',
    )
    const prose = 'Første linje\n<b>Anden linje</b>'
    get.mockImplementation(async (url) =>
      response(
        String(url).split('/').pop()!,
        url === '/SiteTexts/home'
          ? Object.fromEntries(fields.map((field) => [field.key, prose]))
          : {},
      ),
    )
    const wrapper = render()
    await flushPromises()
    const paragraphs = wrapper.findAll('p').filter((p) => p.element.textContent === prose)
    expect(paragraphs).toHaveLength(fields.length)
    expect(paragraphs.every((p) => p.classes().includes('whitespace-pre-line'))).toBe(true)
    expect(wrapper.find('b').exists()).toBe(false)
  })

  it('gates both actions to Admin and explains the shared group inside its editor', async () => {
    const wrapper = render()
    await flushPromises()
    for (const group of groups) expect(action(wrapper, group)).toBeUndefined()
    const auth = admin()
    auth.user!.roles = ['Member']
    await flushPromises()
    for (const group of groups) expect(action(wrapper, group)).toBeUndefined()
    auth.user!.roles = ['Admin']
    await flushPromises()
    await open(wrapper, 'membership')
    expect(ui().get('[role="dialog"]').text()).toContain(
      'både på forsiden og på siden Medlemsfordele',
    )
    expect(ui().get('[role="dialog"]').text()).toContain('gemmes separat')
    expect(ui().findAll('input')).toHaveLength(5)
    auth.clearAuth()
    await flushPromises()
    for (const group of groups) expect(action(wrapper, group)).toBeUndefined()
    expect(ui().get('button[type="submit"]').attributes('disabled')).toBeDefined()
  })

  it.each(groups)(
    'publishes a multi-field %s save immediately in one group-only PATCH',
    async (group) => {
      admin()
      const wrapper = render()
      await flushPromises()
      await open(wrapper, group)
      const keys =
        group === 'home' ? ['hero.title', 'network.action-label'] : benefitKeys.slice(0, 2)
      const changes = Object.fromEntries(keys.map((key) => [key, `Gemt ${key}`]))
      for (const [key, value] of Object.entries(changes)) await field(key).setValue(value)
      expect(wrapper.text()).not.toContain(changes[keys[0]])
      patch.mockResolvedValueOnce(response(group, changes))
      await ui().get('[role="dialog"] form').trigger('submit')
      await flushPromises()
      expect(patch).toHaveBeenCalledExactlyOnceWith(`/SiteTexts/${group}`, { changes })
      for (const value of Object.values(changes)) expect(wrapper.text()).toContain(value)
      const other = group === 'home' ? 'membership' : 'home'
      for (const value of Object.values(defaults[other])) expect(wrapper.text()).toContain(value)
      await vi.waitFor(() => expect(ui().find('[role="dialog"]').exists()).toBe(false))
    },
  )

  it.each(groups)(
    'does not publish cancelled/failed %s drafts or allow saving a failed load',
    async (group) => {
      admin()
      const wrapper = render()
      await flushPromises()
      const key = group === 'home' ? 'hero.title' : benefitKeys[0]
      await open(wrapper, group)
      await field(key).setValue('Kasseret kladde')
      await cancel()
      expect(wrapper.text()).not.toContain('Kasseret kladde')
      expect(patch).not.toHaveBeenCalled()
      await open(wrapper, group)
      expect((field(key).element as HTMLInputElement).value).toBe(defaults[group][key])
      await field(key).setValue('Behold kladde')
      patch.mockRejectedValueOnce(new Error('offline'))
      await ui().get('[role="dialog"] form').trigger('submit')
      await flushPromises()
      expect((field(key).element as HTMLInputElement).value).toBe('Behold kladde')
      expect(ui().get('[role="alert"]').text()).toContain('Dine ændringer er bevaret')
      expect(wrapper.text()).not.toContain('Behold kladde')
      await cancel()
      get.mockRejectedValueOnce(new Error('offline'))
      await open(wrapper, group)
      expect(ui().get('[role="alert"]').text()).toContain('kunne ikke hentes')
      expect(ui().get('button[type="submit"]').attributes('disabled')).toBeDefined()
      expect(wrapper.text()).toContain(defaults[group][key])
    },
  )

  it('keeps all four CTA destinations and smooth scrolling when labels change', async () => {
    const keys = [
      'network.action-label',
      'development.action-label',
      'reasons.action-label',
      'plan.action-label',
    ]
    get.mockImplementation(async (url) =>
      response(
        String(url).split('/').pop()!,
        url === '/SiteTexts/home' ? Object.fromEntries(keys.map((key) => [key, `Ny ${key}`])) : {},
      ),
    )
    const wrapper = render()
    await flushPromises()
    const buttons = wrapper.get('.home-text').findAll('button')
    expect(buttons.map((button) => button.text())).toEqual(keys.map((key) => `Ny ${key}`))
    for (const [index, button] of buttons.entries()) {
      await button.trigger('click')
      await flushPromises()
      expect(push).toHaveBeenNthCalledWith(index + 1, {
        name: index === 0 ? 'advantages' : 'register',
      })
      expect(window.scrollTo).toHaveBeenNthCalledWith(index + 1, { top: 0, behavior: 'smooth' })
    }
  })
})
