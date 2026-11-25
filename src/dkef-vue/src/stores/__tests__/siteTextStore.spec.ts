import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createApp, defineComponent, h, nextTick, onMounted } from 'vue'
import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import persistedState from 'pinia-plugin-persistedstate'
import { AxiosHeaders, type AxiosResponse } from 'axios'
import apiservice from '@/services/apiservice'
import { getSiteTextGroup, getSiteTextDefaults } from '@/content/siteTexts'
import { useSiteTextStore } from '../siteTextStore'

vi.mock('@/services/apiservice', () => ({ default: { get: vi.fn(), patch: vi.fn() } }))
vi.mock('@/services/urlservice', () => ({
  default: {
    getSiteTexts: (group: string) => `/SiteTexts/${group}`,
    patchSiteTexts: (group: string) => `/SiteTexts/${group}`,
  },
}))

const get = vi.mocked(apiservice.get)
const patch = vi.mocked(apiservice.patch)
const defaults = getSiteTextDefaults(getSiteTextGroup('home')!)
const wireResponse = (data: unknown): AxiosResponse => ({
  data,
  status: 200,
  statusText: 'OK',
  headers: new AxiosHeaders(),
  config: { headers: new AxiosHeaders() },
})
const response = (overrides = {}, group = 'home') => wireResponse({ group, overrides })
function deferred<T>() {
  let resolve!: (value: T) => void
  let reject!: (reason: unknown) => void
  const promise = new Promise<T>((res, rej) => {
    resolve = res
    reject = rej
  })
  return { promise, resolve, reject }
}

beforeEach(() => {
  vi.resetAllMocks()
  setActivePinia(createPinia())
})

describe('shared site text state', () => {
  it('displays defaults immediately, distinguishes failed/empty loads, and retries', async () => {
    const store = useSiteTextStore()
    expect(store.groups.home.loadStatus).toBe('idle')
    expect(store.texts.home).toEqual(defaults)
    get.mockRejectedValueOnce(new Error('offline'))
    await expect(store.loadGroup('home')).rejects.toThrow('offline')
    expect(store.groups.home.loadStatus).toBe('error')
    expect(store.groups.home.loadError).toBeInstanceOf(Error)
    expect(store.texts.home).toEqual(defaults)
    get.mockResolvedValueOnce(response())
    await store.loadGroup('home')
    expect(store.groups.home.loadStatus).toBe('loaded')
    expect(store.groups.home.loadError).toBeNull()
    await store.loadGroup('home')
    expect(get).toHaveBeenCalledTimes(2)
    expect(get).toHaveBeenLastCalledWith('/SiteTexts/home', {
      skipAuth: true,
      withCredentials: false,
    })
  })

  it('deduplicates mounted consumers and publishes reactive values without sharing drafts', async () => {
    const pending = deferred<ReturnType<typeof response>>()
    get.mockReturnValueOnce(pending.promise as ReturnType<typeof apiservice.get>)
    const first = useSiteTextStore()
    const second = useSiteTextStore()
    const loads = [first.loadGroup('home'), second.loadGroup('home')]
    await Promise.resolve()
    expect(get).toHaveBeenCalledTimes(1)
    pending.resolve(response({ 'hero.title': 'Fælles titel' }))
    await Promise.all(loads)
    expect(second.texts.home['hero.title']).toBe('Fælles titel')
    const draft = first.createDraft('home')
    draft['hero.title'] = 'Ikke udgivet'
    expect(second.texts.home['hero.title']).toBe('Fælles titel')
  })

  it('resolves an empty override by presence, not truthiness (future optional field)', async () => {
    // The response boundary accepts strings; required/optional validation belongs to the API.
    get.mockResolvedValueOnce(response({ 'hero.title': '' }))
    const store = useSiteTextStore()
    await store.loadGroup('home')
    expect(store.texts.home['hero.title']).toBe('')
  })

  it('updates every mounted consumer after loading and saving the shared group', async () => {
    const store = useSiteTextStore()
    const pending = deferred<ReturnType<typeof response>>()
    get.mockReturnValueOnce(pending.promise)
    const loads: Promise<unknown>[] = []
    const Consumer = defineComponent({
      setup() {
        const shared = useSiteTextStore()
        onMounted(() => {
          loads.push(shared.loadGroup('home'))
        })
        return () => h('p', shared.texts.home['hero.title'])
      },
    })
    const first = mount(Consumer)
    const second = mount(Consumer)
    try {
      expect(first.text()).toBe(defaults['hero.title'])
      await Promise.resolve()
      expect(get).toHaveBeenCalledTimes(1)
      pending.resolve(response({ 'hero.title': 'Indlæst' }))
      await Promise.all(loads)
      await nextTick()
      expect(first.text()).toBe('Indlæst')
      expect(second.text()).toBe('Indlæst')
      get.mockResolvedValueOnce(response({ 'hero.title': 'Indlæst' }))
      const original = await store.refreshForEditor('home')
      patch.mockResolvedValueOnce(response({ 'hero.title': 'Gemt' }))
      await store.saveGroup('home', { ...original, 'hero.title': 'Gemt' }, original)
      await nextTick()
      expect(first.text()).toBe('Gemt')
      expect(second.text()).toBe('Gemt')
    } finally {
      first.unmount()
      second.unmount()
    }
  })

  it('requires an editor refresh and blocks saves after a failed refresh while retaining old overrides', async () => {
    const store = useSiteTextStore()
    get.mockResolvedValueOnce(response({ 'hero.title': 'Udgivet' }))
    await store.loadGroup('home')
    const original = store.createDraft('home')
    const draft = { ...original, 'hero.title': 'Ny' }
    await expect(store.saveGroup('home', draft, original)).rejects.toThrow('Hent teksterne igen')
    get.mockRejectedValueOnce(new Error('offline'))
    await expect(store.refreshForEditor('home')).rejects.toThrow('offline')
    expect(store.texts.home['hero.title']).toBe('Udgivet')
    expect(store.groups.home.editorReady).toBe(false)
    await expect(store.saveGroup('home', draft, original)).rejects.toThrow('Hent teksterne igen')
    expect(patch).not.toHaveBeenCalled()
  })

  it('sends only edited keys, then replaces overrides with the complete authoritative response', async () => {
    const store = useSiteTextStore()
    get.mockResolvedValueOnce(
      response({ 'hero.title': 'Gammel', 'hero.description': 'Fjernet override' }),
    )
    const original = await store.refreshForEditor('home')
    const draft = { ...original, 'hero.title': '<b>Æble</b>' }
    patch.mockResolvedValueOnce(
      response({ 'hero.title': 'Æble', 'hero.description': 'Anden administrators tekst' }),
    )
    await store.saveGroup('home', draft, original)
    expect(patch).toHaveBeenCalledExactlyOnceWith('/SiteTexts/home', {
      changes: { 'hero.title': '<b>Æble</b>' },
    })
    expect(store.texts.home['hero.title']).toBe('Æble')
    expect(store.texts.home['hero.description']).toBe('Anden administrators tekst')
    expect(draft['hero.title']).toBe('<b>Æble</b>')
    expect(original['hero.title']).toBe('Gammel')
    expect(store.groups.home.saveStatus).toBe('saved')
  })

  it('does not PATCH unchanged drafts and rejects unregistered fields', async () => {
    const store = useSiteTextStore()
    get.mockResolvedValueOnce(response())
    const original = await store.refreshForEditor('home')
    await store.saveGroup('home', { ...original }, original)
    await expect(store.saveGroup('home', { bad: 'tekst' }, original)).rejects.toThrow(
      'ugyldigt felt',
    )
    expect(patch).not.toHaveBeenCalled()
  })

  it('blocks saving during editor refresh and sends a changed default-equivalent value as an override', async () => {
    const store = useSiteTextStore()
    get.mockResolvedValueOnce(response({ 'hero.title': 'Tidligere tekst' }))
    const original = await store.refreshForEditor('home')
    const pending = deferred<ReturnType<typeof response>>()
    get.mockReturnValueOnce(pending.promise)
    const refresh = store.refreshForEditor('home')
    await expect(store.saveGroup('home', defaults, original)).rejects.toThrow('Hent teksterne igen')
    expect(patch).not.toHaveBeenCalled()
    pending.resolve(response({ 'hero.title': 'Tidligere tekst' }))
    const current = await refresh
    patch.mockResolvedValueOnce(response({ 'hero.title': defaults['hero.title'] }))
    await store.saveGroup('home', defaults, current)
    expect(patch).toHaveBeenCalledWith('/SiteTexts/home', {
      changes: { 'hero.title': defaults['hero.title'] },
    })
    expect(store.groups.home.overrides['hero.title']).toBe(defaults['hero.title'])
  })

  it.each([400, 401, 403, 404, 413, 500])(
    'retains drafts and published state after PATCH %s',
    async (status) => {
      const store = useSiteTextStore()
      get.mockResolvedValueOnce(response({ 'hero.title': 'Udgivet' }))
      const original = await store.refreshForEditor('home')
      const draft = { ...original, 'hero.title': 'Kladde' }
      const failure = { response: { status } }
      patch.mockRejectedValueOnce(failure)
      await expect(store.saveGroup('home', draft, original)).rejects.toBe(failure)
      expect(store.texts.home['hero.title']).toBe('Udgivet')
      expect(draft['hero.title']).toBe('Kladde')
      expect(store.groups.home.saveStatus).toBe('error')
      expect(store.groups.home.saveError).toEqual(failure)
      patch.mockResolvedValueOnce(response({ 'hero.title': 'Kladde' }))
      await store.saveGroup('home', draft, original)
      expect(store.groups.home.saveError).toBeNull()
    },
  )

  it('disables overlapping same-group saves without optimistic publication', async () => {
    const store = useSiteTextStore()
    get.mockResolvedValueOnce(response())
    const original = await store.refreshForEditor('home')
    const pending = deferred<ReturnType<typeof response>>()
    patch.mockReturnValueOnce(pending.promise as ReturnType<typeof apiservice.patch>)
    const draft = { ...original, 'hero.title': 'Ny' }
    const save = store.saveGroup('home', draft, original)
    expect(store.texts.home).toEqual(defaults)
    await expect(store.saveGroup('home', draft, original)).rejects.toThrow('allerede')
    pending.resolve(response({ 'hero.title': 'Ny' }))
    await save
    expect(patch).toHaveBeenCalledTimes(1)
  })

  it('ignores a delayed GET after a newer successful save', async () => {
    const store = useSiteTextStore()
    get.mockResolvedValueOnce(response())
    const original = await store.refreshForEditor('home')
    const pendingSave = deferred<ReturnType<typeof response>>()
    const pendingGet = deferred<ReturnType<typeof response>>()
    patch.mockReturnValueOnce(pendingSave.promise as ReturnType<typeof apiservice.patch>)
    const save = store.saveGroup('home', { ...original, 'hero.title': 'Ny' }, original)
    get.mockReturnValueOnce(pendingGet.promise as ReturnType<typeof apiservice.get>)
    const load = store.refreshGroup('home')
    await Promise.resolve()
    pendingSave.resolve(response({ 'hero.title': 'Ny' }))
    await save
    pendingGet.resolve(response({ 'hero.title': 'Forældet' }))
    await load
    expect(store.texts.home['hero.title']).toBe('Ny')
    expect(store.groups.home.loadStatus).toBe('loaded')
  })

  it('ignores older load failures and rejects superseded editor refreshes', async () => {
    const store = useSiteTextStore()
    const old = deferred<ReturnType<typeof response>>()
    get.mockReturnValueOnce(old.promise as ReturnType<typeof apiservice.get>)
    const first = store.refreshForEditor('home')
    await Promise.resolve()
    get.mockResolvedValueOnce(response({ 'hero.title': 'Aktuel' }))
    await store.refreshForEditor('home')
    old.resolve(response({ 'hero.title': 'Forældet' }))
    await expect(first).rejects.toThrow('erstattet')
    expect(store.groups.home.editorReady).toBe(true)
    expect(store.texts.home['hero.title']).toBe('Aktuel')
  })

  it('does not let an older public failure mark a newer editor load as failed', async () => {
    const store = useSiteTextStore()
    const old = deferred<ReturnType<typeof response>>()
    get.mockReturnValueOnce(old.promise)
    const load = store.loadGroup('home')
    await Promise.resolve()
    get.mockResolvedValueOnce(response({ 'hero.title': 'Aktuel' }))
    await store.refreshForEditor('home')
    old.reject(new Error('old failure'))
    await expect(load).rejects.toThrow('old failure')
    expect(store.groups.home.loadStatus).toBe('loaded')
    expect(store.groups.home.loadError).toBeNull()
    expect(store.groups.home.editorReady).toBe(true)
  })

  it('replaces rather than merges overrides after a successful PATCH', async () => {
    const store = useSiteTextStore()
    get.mockResolvedValueOnce(
      response({ 'hero.title': 'Titel', 'hero.description': 'Tidligere override' }),
    )
    const original = await store.refreshForEditor('home')
    patch.mockResolvedValueOnce(response({ 'hero.title': 'Ny' }))
    await store.saveGroup('home', { ...original, 'hero.title': 'Ny' }, original)
    expect(store.texts.home['hero.description']).toBe(defaults['hero.description'])
    expect(store.groups.home.overrides).toEqual({ 'hero.title': 'Ny' })
  })

  it.each([
    null,
    [],
    {},
    { group: 'about', overrides: {} },
    { group: 'home', overrides: [] },
    { group: 'home', overrides: { bad: 'tekst' } },
    { group: 'home', overrides: { 'hero.title': null } },
  ])('rejects malformed GET/PATCH data: %j', async (data) => {
    const store = useSiteTextStore()
    get.mockResolvedValueOnce(wireResponse(data))
    await expect(store.loadGroup('home')).rejects.toThrow('ugyldigt svar')
    expect(store.texts.home).toEqual(defaults)
    get.mockResolvedValueOnce(response())
    const original = await store.refreshForEditor('home')
    patch.mockResolvedValueOnce(wireResponse(data))
    await expect(
      store.saveGroup('home', { ...original, 'hero.title': 'Ny' }, original),
    ).rejects.toThrow('ugyldigt svar')
    expect(store.texts.home).toEqual(defaults)
  })

  it('isolates groups and refuses unknown groups without network access', async () => {
    const store = useSiteTextStore()
    get.mockResolvedValueOnce(response({ 'hero.title': 'Hjem' }))
    await store.loadGroup('home')
    expect(store.groups.about.loadStatus).toBe('idle')
    expect(() => store.loadGroup('HOME')).toThrow('findes ikke')
    expect(get).toHaveBeenCalledTimes(1)
  })

  it('does not opt into browser persistence even with the application plugin installed', async () => {
    const storage = { getItem: vi.fn(() => null), setItem: vi.fn() }
    const pinia = createPinia().use(persistedState)
    createApp({}).use(pinia)
    // Use real Web Storage where available; no storage calls should occur for this store.
    vi.stubGlobal('localStorage', storage)
    vi.stubGlobal('sessionStorage', storage)
    try {
      const store = useSiteTextStore(pinia)
      get.mockResolvedValueOnce(response({ 'hero.title': 'Kun i hukommelsen' }))
      await store.loadGroup('home')
      await nextTick()
      expect(storage.getItem).not.toHaveBeenCalled()
      expect(storage.setItem).not.toHaveBeenCalled()
      expect(useSiteTextStore(createPinia()).groups.home.loadStatus).toBe('idle')
    } finally {
      vi.unstubAllGlobals()
    }
  })
})
