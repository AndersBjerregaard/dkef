import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { DOMWrapper, flushPromises, mount, type VueWrapper } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import { AxiosHeaders, type AxiosResponse } from 'axios'
import MemberAdvantages from '../MemberAdvantages.vue'
import HomeComponent from '../HomeComponent.vue'
import { getSiteTextDefaults, getSiteTextFields, getSiteTextGroup } from '@/content/siteTexts'
import { useAuthStore } from '@/stores/authStore'
import apiservice from '@/services/apiservice'

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
const groups = ['member-advantages', 'membership'] as const
const defaults = Object.fromEntries(
  [...groups, 'home'].map((group) => [group, getSiteTextDefaults(getSiteTextGroup(group)!)]),
)
const benefitKeys = [
  'benefits.development',
  'benefits.network',
  'benefits.mentoring',
  'benefits.events',
  'benefits.magazine',
]
const labels = {
  'member-advantages': 'Rediger sidens tekster',
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
function render(component = MemberAdvantages) {
  const wrapper = mount(component, {
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

describe('Member Advantages editable text integration', () => {
  it.each(['empty', 'failure'])('keeps every default before and after %s loads', async (load) => {
    if (load === 'failure') get.mockRejectedValue(new Error('offline'))
    const wrapper = render()
    for (const group of groups)
      for (const value of Object.values(defaults[group])) expect(wrapper.text()).toContain(value)
    await flushPromises()
    for (const group of groups) {
      for (const value of Object.values(defaults[group])) expect(wrapper.text()).toContain(value)
      expect(get).toHaveBeenCalledWith(`/SiteTexts/${group}`, {
        skipAuth: true,
        withCredentials: false,
      })
    }
    expect(get).toHaveBeenCalledTimes(2)
  })

  it('binds all 31 slots independently, preserving fixed lists, cards, media and dividers', async () => {
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
    const page = overrides['member-advantages']
    const slots = wrapper.findAll('h1, h2, p, button')
    expect(slots.map((slot) => slot.text())).toEqual([
      ...[
        'page.title',
        'introduction.eyebrow',
        'introduction.title',
        'introduction.description',
        'introduction.action-label',
        'membership.title',
      ].map((key) => page[key]),
      ...benefitKeys.map((key) => `✅ ${overrides.membership[key]}`),
      page['student-offers.title'],
      ...['mentoring', 'network-meetings', 'project-network', 'jobs', 'included-benefits'].map(
        (key) => `•${page[`student-offers.${key}`]}`,
      ),
      ...[
        'network.eyebrow',
        'network.title',
        'network.description',
        'events.eyebrow',
        'events.title',
        'events.description',
        'other-benefits.eyebrow',
        'other-benefits.title',
        'mentoring.title',
        'mentoring.description',
        'sparring.title',
        'sparring.description',
        'magazine.title',
        'magazine.description',
      ].map((key) => page[key]),
    ])
    expect(slots.map((slot) => slot.element.tagName)).toEqual([
      'H1',
      'P',
      'H1',
      'P',
      'BUTTON',
      'H2',
      'P',
      'P',
      'P',
      'P',
      'P',
      'H2',
      'P',
      'P',
      'P',
      'P',
      'P',
      'P',
      'H1',
      'P',
      'P',
      'H1',
      'P',
      'P',
      'H1',
      'H2',
      'P',
      'H2',
      'P',
      'H2',
      'P',
    ])
    expect(wrapper.findAll('div.w-72')).toHaveLength(3)
    expect(wrapper.findAll('div.w-full.border-t')).toHaveLength(3)
    expect(wrapper.findAll('img').map((img) => img.attributes('src'))).toEqual(
      [1, 2, 3, 4, 5].map((n) => `/src/assets/advantages${n}.jpeg`),
    )
    expect(wrapper.findAll('img').map((img) => img.attributes('alt'))).toEqual([
      'Fagligt netværk',
      'Arrangement for medlemmer',
      'Mentorordning',
      'Faglig sparring',
      'Faglig viden og inspiration',
    ])
    expect(wrapper.findAll('p.flex > span:first-child').map((span) => span.text())).toEqual(
      Array(5).fill('•'),
    )
  })

  it.each(groups)('loads %s independently when the other group fails', async (group) => {
    const key = group === 'membership' ? benefitKeys[0] : 'page.title'
    get.mockImplementation(async (url) => {
      if (url !== `/SiteTexts/${group}`) throw new Error('offline')
      return response(group, { [key]: 'Indlæst tekst' })
    })
    const wrapper = render()
    await flushPromises()
    expect(wrapper.text()).toContain('Indlæst tekst')
    const other = groups.find((candidate) => candidate !== group)!
    for (const value of Object.values(defaults[other])) expect(wrapper.text()).toContain(value)
  })

  it('renders literal markup and preserves line breaks only in the six approved multiline slots', async () => {
    const fields = getSiteTextFields(getSiteTextGroup('member-advantages')!)
    const prose = 'Første linje\n<b>Anden linje</b>'
    get.mockImplementation(async (url) =>
      response(
        String(url).split('/').pop()!,
        url === '/SiteTexts/member-advantages'
          ? Object.fromEntries(
              fields.map((field) => [
                field.key,
                field.input === 'multiline' ? prose : '<b>Tekst</b>',
              ]),
            )
          : {},
      ),
    )
    const wrapper = render()
    await flushPromises()
    const multiline = wrapper.findAll('.whitespace-pre-line')
    expect(multiline).toHaveLength(6)
    expect(multiline.every((slot) => slot.element.textContent === prose)).toBe(true)
    expect(wrapper.find('b').exists()).toBe(false)
    expect(wrapper.get('h1').text()).toBe('<b>Tekst</b>')
  })

  it('reactively gates both actions to Admin and explains shared publication', async () => {
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

  it.each(['advantages', 'home'])(
    'publishes shared benefits from %s to both mounted consumers without sharing page copy',
    async (source) => {
      admin()
      const advantages = render()
      const home = render(HomeComponent)
      await flushPromises()
      expect(get.mock.calls.filter(([url]) => url === '/SiteTexts/membership')).toHaveLength(1)
      await open(source === 'home' ? home : advantages, 'membership')
      const changes = {
        'benefits.mentoring': 'Fælles mentorfordel',
        'benefits.network': 'Fælles netværk',
      }
      for (const [key, value] of Object.entries(changes)) await field(key).setValue(value)
      for (const wrapper of [home, advantages])
        expect(wrapper.text()).not.toContain('Fælles mentorfordel')
      patch.mockResolvedValueOnce(response('membership', changes))
      await ui().get('form').trigger('submit')
      await flushPromises()
      expect(patch).toHaveBeenCalledExactlyOnceWith('/SiteTexts/membership', { changes })
      for (const wrapper of [home, advantages])
        for (const value of Object.values(changes)) expect(wrapper.text()).toContain(value)
      expect(advantages.findAll('p.flex')[0].get('span:last-child').text()).toBe('Mentorordning')
      for (const group of ['home', 'member-advantages'])
        for (const value of Object.values(defaults[group]))
          expect((group === 'home' ? home : advantages).text()).toContain(value)
      await vi.waitFor(() => expect(ui().find('[role="dialog"]').exists()).toBe(false))
      await open(advantages, 'member-advantages')
      const pageChanges = {
        'student-offers.mentoring': 'Kun for studerende',
        'sparring.description': 'Kun på denne side',
      }
      for (const [key, value] of Object.entries(pageChanges)) await field(key).setValue(value)
      patch.mockResolvedValueOnce(response('member-advantages', pageChanges))
      await ui().get('form').trigger('submit')
      await flushPromises()
      expect(patch).toHaveBeenLastCalledWith('/SiteTexts/member-advantages', {
        changes: pageChanges,
      })
      expect(patch).toHaveBeenCalledTimes(2)
      for (const value of Object.values(pageChanges)) {
        expect(advantages.text()).toContain(value)
        expect(home.text()).not.toContain(value)
      }
      for (const wrapper of [home, advantages])
        expect(wrapper.text()).toContain('Fælles mentorfordel')
    },
  )

  it.each(groups)(
    'isolates cancelled/failed %s drafts and blocks saves after failed editor loads',
    async (group) => {
      admin()
      const wrapper = render()
      await flushPromises()
      const key = group === 'membership' ? benefitKeys[0] : 'page.title'
      await open(wrapper, group)
      await field(key).setValue('Kasseret kladde')
      await cancel()
      expect(wrapper.text()).not.toContain('Kasseret kladde')
      expect(patch).not.toHaveBeenCalled()
      await open(wrapper, group)
      expect((field(key).element as HTMLInputElement).value).toBe(defaults[group][key])
      await field(key).setValue('Behold kladde')
      patch.mockRejectedValueOnce(new Error('offline'))
      await ui().get('form').trigger('submit')
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

  it('uses the approved field limits and blocks oversized drafts', async () => {
    admin()
    const wrapper = render()
    await flushPromises()
    for (const group of groups) {
      await open(wrapper, group)
      const fields = getSiteTextFields(getSiteTextGroup(group)!)
      for (const definition of fields) {
        expect(field(definition.key).attributes('maxlength')).toBe(String(definition.maxLength))
        await field(definition.key).setValue('W'.repeat(definition.maxLength))
      }
      expect(ui().get('button[type="submit"]').attributes('disabled')).toBeUndefined()
      await field(fields[0].key).setValue('W'.repeat(fields[0].maxLength + 1))
      expect(ui().get('button[type="submit"]').attributes('disabled')).toBeDefined()
      await ui().get('form').trigger('submit')
      expect(patch).not.toHaveBeenCalled()
      await cancel()
    }
  })

  it('keeps register navigation and smooth scrolling when the CTA label changes', async () => {
    get.mockImplementation(async (url) =>
      response(
        String(url).split('/').pop()!,
        url === '/SiteTexts/member-advantages'
          ? { 'introduction.action-label': 'Tilmeld dig nu' }
          : {},
      ),
    )
    const wrapper = render()
    await flushPromises()
    await wrapper.get('button').trigger('click')
    await flushPromises()
    expect(wrapper.get('button').text()).toBe('Tilmeld dig nu')
    expect(push).toHaveBeenCalledExactlyOnceWith({ name: 'register' })
    expect(window.scrollTo).toHaveBeenCalledExactlyOnceWith({ top: 0, behavior: 'smooth' })
  })
})
