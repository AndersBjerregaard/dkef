import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { defineComponent, h, nextTick } from 'vue'
import { DOMWrapper, flushPromises, mount, type VueWrapper } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import { AxiosHeaders, type AxiosResponse } from 'axios'
import { toast } from 'vue-sonner'
import SiteTextEditorModal from '../SiteTextEditorModal.vue'
import SiteTextEditAction from '../SiteTextEditAction.vue'
import { useAuthStore } from '@/stores/authStore'
import { useSiteTextStore } from '@/stores/siteTextStore'
import { getSiteTextFields, getSiteTextGroup, siteTextRegistry } from '@/content/siteTexts'
import apiservice from '@/services/apiservice'

vi.mock('@/services/apiservice', () => ({ default: { get: vi.fn(), patch: vi.fn() } }))
vi.mock('@/services/urlservice', () => ({
  default: {
    getSiteTexts: (group: string) => `/SiteTexts/${group}`,
    patchSiteTexts: (group: string) => `/SiteTexts/${group}`,
  },
}))
vi.mock('vue-sonner', () => ({ toast: { success: vi.fn() } }))

const get = vi.mocked(apiservice.get)
const patch = vi.mocked(apiservice.patch)
const wrappers: VueWrapper[] = []
const response = (overrides = {}, group = 'about'): AxiosResponse => ({
  data: { group, overrides },
  status: 200,
  statusText: 'OK',
  headers: new AxiosHeaders(),
  config: { headers: new AxiosHeaders() },
})
function deferred<T>() {
  let resolve!: (value: T) => void
  let reject!: (reason: unknown) => void
  const promise = new Promise<T>((res, rej) => {
    resolve = res
    reject = rej
  })
  return { promise, resolve, reject }
}
function admin() {
  const auth = useAuthStore()
  auth.accessToken = 'test-token'
  auth.user = { email: 'admin@example.test', name: 'Admin', roles: ['Admin'] }
  return auth
}
function render(group = 'about') {
  const wrapper = mount(SiteTextEditorModal, {
    props: { group, isOpen: true },
    attachTo: document.body,
    global: { stubs: { teleport: false, transition: false } },
  })
  wrappers.push(wrapper)
  return wrapper
}
// Headless UI portals the real dialog outside the mounted component's root.
const ui = () => new DOMWrapper(document.body)
const dialog = (wrapper: VueWrapper) => new DOMWrapper(wrapper.element.ownerDocument!.body)
const input = (wrapper: VueWrapper, key = 'page.title') => dialog(wrapper).get(`[id$="-${key}"]`)
const saveButton = (wrapper: VueWrapper) => dialog(wrapper).get('button[type="submit"]')
const submit = async (wrapper: VueWrapper) => {
  await dialog(wrapper).get('form').trigger('submit')
  await flushPromises()
}

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
  admin()
  get.mockResolvedValue(response())
})
afterEach(() => {
  wrappers.splice(0).forEach((wrapper) => wrapper.unmount())
  document.body.innerHTML = ''
  vi.unstubAllGlobals()
})

describe('site-text editor with the real BaseModal', () => {
  it.each(siteTextRegistry.groups.map((group) => [group.id]))(
    'renders registry order and Danish labels for %s',
    async (group) => {
      get.mockResolvedValue(response({}, group))
      const wrapper = render(group)
      await flushPromises()
      const definition = getSiteTextGroup(group)!
      expect(
        ui()
          .findAll('legend')
          .map((legend) => legend.text()),
      ).toEqual(definition.sections.map((section) => section.label))
      expect(
        ui()
          .findAll('label')
          .map((label) => label.text().replace(/ \*$/, '')),
      ).toEqual(getSiteTextFields(definition).map((field) => field.label))
      for (const field of getSiteTextFields(definition)) {
        const control = input(wrapper, field.key)
        expect(control.element.tagName).toBe(field.input === 'multiline' ? 'TEXTAREA' : 'INPUT')
        expect(
          ui()
            .findAll('label')
            .some((label) => label.attributes('for') === control.attributes('id')),
        ).toBe(true)
        expect(control.attributes('aria-describedby')).toContain(`${control.attributes('id')}-hint`)
      }
      expect(saveButton(wrapper).attributes('disabled')).toBeDefined()
      await submit(wrapper)
      expect(patch).not.toHaveBeenCalled()
    },
  )

  it('forces a fresh load, blocks failed loads even with cached text, and retries empty overrides', async () => {
    const store = useSiteTextStore()
    get.mockResolvedValueOnce(response({ 'page.title': 'Allerede udgivet' }))
    await store.loadGroup('about')
    get.mockRejectedValueOnce(new Error('offline'))
    const wrapper = render()
    await flushPromises()
    expect(ui().text()).toContain('Teksterne kunne ikke hentes')
    expect(ui().findAll('input')).toHaveLength(0)
    expect(saveButton(wrapper).attributes('disabled')).toBeDefined()
    expect(store.texts.about['page.title']).toBe('Allerede udgivet')
    await ui()
      .findAll('button')
      .find((button) => button.text() === 'Prøv igen')!
      .trigger('click')
    await flushPromises()
    expect(get).toHaveBeenCalledTimes(3)
    expect((input(wrapper).element as HTMLInputElement).value).toBe('Om Elektroteknisk forening')
    await input(wrapper).setValue('Ny titel')
    expect(saveButton(wrapper).attributes('disabled')).toBeUndefined()
  })

  it('isolates drafts and preserves the original baseline when published state changes elsewhere', async () => {
    const wrapper = render()
    await flushPromises()
    const store = useSiteTextStore()
    await input(wrapper).setValue('Min titel')
    expect(store.texts.about['page.title']).toBe('Om Elektroteknisk forening')
    get.mockResolvedValueOnce(response({ 'page.subtitle': 'En andens underoverskrift' }))
    await store.refreshForEditor('about')
    expect((input(wrapper).element as HTMLInputElement).value).toBe('Min titel')
    expect((input(wrapper, 'page.subtitle').element as HTMLInputElement).value).toBe(
      'Vi sætter strøm til elteknik-branchen',
    )
    patch.mockResolvedValue(
      response({ 'page.title': 'Min titel', 'page.subtitle': 'En andens underoverskrift' }),
    )
    await submit(wrapper)
    expect(patch).toHaveBeenCalledWith('/SiteTexts/about', {
      changes: { 'page.title': 'Min titel' },
    })
  })

  it.each(['cancel', 'close', 'escape', 'backdrop'])(
    'discards edits through %s and refreshes on reopening',
    async (path) => {
      const wrapper = render()
      await flushPromises()
      await input(wrapper).setValue('Ikke udgivet')
      if (path === 'cancel')
        await ui()
          .findAll('button')
          .find((button) => button.text() === 'Annuller')!
          .trigger('click')
      if (path === 'close')
        await ui()
          .findAll('button')
          .find((button) => button.text() === 'Luk')!
          .trigger('click')
      if (path === 'escape') {
        input(wrapper).element.dispatchEvent(
          new KeyboardEvent('keydown', { key: 'Escape', bubbles: true }),
        )
        await flushPromises()
      }
      if (path === 'backdrop') {
        const backdrop = ui().get('.fixed.inset-0.overflow-y-auto')
        await backdrop.trigger('mousedown')
        await backdrop.trigger('click')
        await flushPromises()
      }
      expect(wrapper.emitted('close')).toHaveLength(1)
      expect(patch).not.toHaveBeenCalled()
      expect(useSiteTextStore().texts.about['page.title']).toBe('Om Elektroteknisk forening')
      await wrapper.setProps({ isOpen: false })
      get.mockResolvedValueOnce(response({ 'page.title': 'Ny udgivet titel' }))
      await wrapper.setProps({ isOpen: true })
      await flushPromises()
      expect((input(wrapper).element as HTMLInputElement).value).toBe('Ny udgivet titel')
      expect(saveButton(wrapper).attributes('disabled')).toBeDefined()
    },
  )

  it('ignores a late load after closing and reopening', async () => {
    const pending = deferred<AxiosResponse>()
    get.mockReturnValueOnce(pending.promise)
    const wrapper = render()
    await flushPromises()
    expect(ui().text()).toContain('Henter tekster')
    await wrapper.setProps({ isOpen: false })
    get.mockResolvedValueOnce(response({ 'page.title': 'Aktuel titel' }))
    await wrapper.setProps({ isOpen: true })
    await flushPromises()
    pending.resolve(response({ 'page.title': 'Forældet titel' }))
    await flushPromises()
    expect((input(wrapper).element as HTMLInputElement).value).toBe('Aktuel titel')
  })

  it('saves dirty fields together, blocks duplicates/dismissal, and publishes sanitized values to consumers', async () => {
    const wrapper = render()
    await flushPromises()
    const Consumer = defineComponent({
      setup: () => {
        const store = useSiteTextStore()
        return () => h('p', store.texts.about['page.title'])
      },
    })
    const consumers = [mount(Consumer), mount(Consumer)]
    wrappers.push(...consumers)
    const pending = deferred<AxiosResponse>()
    patch.mockReturnValueOnce(pending.promise)
    await input(wrapper).setValue('<b>Ny titel</b>')
    await input(wrapper, 'page.subtitle').setValue('Ny underoverskrift')
    await submit(wrapper)
    await submit(wrapper)
    expect(patch).toHaveBeenCalledTimes(1)
    expect(patch).toHaveBeenCalledWith('/SiteTexts/about', {
      changes: { 'page.title': '<b>Ny titel</b>', 'page.subtitle': 'Ny underoverskrift' },
    })
    expect(ui().text()).toContain('Gemmer og offentliggør')
    expect(
      ui()
        .findAll('[role="dialog"] button')
        .every((button) => button.attributes('disabled') !== undefined),
    ).toBe(true)
    input(wrapper).element.dispatchEvent(
      new KeyboardEvent('keydown', { key: 'Escape', bubbles: true }),
    )
    await ui().get('.fixed.inset-0.overflow-y-auto').trigger('mousedown')
    await ui().get('.fixed.inset-0.overflow-y-auto').trigger('click')
    expect(wrapper.emitted('close')).toBeUndefined()
    expect(consumers[0].text()).toBe('Om Elektroteknisk forening')
    pending.resolve(response({ 'page.title': 'Ny titel', 'page.subtitle': 'Ny underoverskrift' }))
    await flushPromises()
    expect(consumers.map((consumer) => consumer.text())).toEqual(['Ny titel', 'Ny titel'])
    expect(toast.success).toHaveBeenCalledWith('Teksterne er gemt og offentliggjort.')
    expect(wrapper.emitted('close')).toHaveLength(1)
  })

  it('validates required, length, email and phone values with accessible inline errors', async () => {
    const wrapper = render()
    await flushPromises()
    for (const [key, value, message] of [
      ['page.title', '  ', 'Feltet skal udfyldes.'],
      ['page.title', 'a'.repeat(61), 'Teksten må højst være 60 tegn.'],
      ['secretariat.email', 'ugyldig', 'Indtast en gyldig e-mailadresse'],
      ['secretariat.phone', 'abc', 'Telefonnummeret må kun indeholde'],
    ]) {
      await input(wrapper, key).setValue(value)
      expect(ui().text()).toContain(message)
      expect(input(wrapper, key).attributes('aria-invalid')).toBe('true')
      expect(input(wrapper, key).attributes('aria-describedby')).toContain('-error')
      await submit(wrapper)
      expect(patch).not.toHaveBeenCalled()
      await input(wrapper, key).setValue(
        getSiteTextFields(getSiteTextGroup('about')!).find((field) => field.key === key)!.default,
      )
    }
  })

  it.each([400, 413, 500, undefined])(
    'preserves every edit on failed save (%s) and allows a retry',
    async (status) => {
      const wrapper = render()
      await flushPromises()
      await input(wrapper).setValue('Min titel')
      await input(wrapper, 'page.subtitle').setValue('Min underoverskrift')
      patch.mockRejectedValueOnce(
        status === 400
          ? {
              response: {
                status,
                data: {
                  errors: {
                    'changes.page.title': ['Titlen blev afvist.'],
                    request: ['Ugyldig anmodning.'],
                  },
                },
              },
            }
          : status
            ? { response: { status } }
            : new Error('Network Error'),
      )
      await submit(wrapper)
      expect(ui().text()).toContain('Dine ændringer er bevaret')
      if (status === 400) {
        expect(ui().text()).toContain('Titlen blev afvist.')
        expect(input(wrapper).attributes('aria-invalid')).toBe('true')
      }
      expect((input(wrapper, 'page.subtitle').element as HTMLInputElement).value).toBe(
        'Min underoverskrift',
      )
      expect(wrapper.emitted('close')).toBeUndefined()
      expect(useSiteTextStore().texts.about['page.title']).toBe('Om Elektroteknisk forening')
      await input(wrapper).setValue('Rettet titel')
      expect(ui().text()).not.toContain('Titlen blev afvist.')
      patch.mockResolvedValueOnce(
        response({ 'page.title': 'Rettet titel', 'page.subtitle': 'Min underoverskrift' }),
      )
      await submit(wrapper)
      expect(wrapper.emitted('close')).toHaveLength(1)
    },
  )

  it.each([401, 403])(
    'blocks further saves after auth failure %s without discarding the draft',
    async (status) => {
      const wrapper = render()
      await flushPromises()
      await input(wrapper).setValue('Bevar mig')
      patch.mockRejectedValueOnce({ response: { status } })
      await submit(wrapper)
      await submit(wrapper)
      expect(patch).toHaveBeenCalledTimes(1)
      expect(saveButton(wrapper).attributes('disabled')).toBeDefined()
      expect(ui().text()).toContain('Log ind igen som administrator')
      expect((input(wrapper).element as HTMLInputElement).value).toBe('Bevar mig')
      expect(wrapper.emitted('close')).toBeUndefined()
      useAuthStore().accessToken = 'new-session'
      await nextTick()
      expect(saveButton(wrapper).attributes('disabled')).toBeUndefined()
    },
  )

  it('uses Danish request-level feedback when validation has no recognized field errors', async () => {
    const wrapper = render()
    await flushPromises()
    await input(wrapper).setValue('Min titel')
    patch.mockRejectedValueOnce({
      response: {
        status: 400,
        data: { errors: { request: ['Request failed'], 'changes.unknown': ['Unknown'] } },
      },
    })
    await submit(wrapper)
    expect(ui().text()).toContain('Teksterne kunne ikke gemmes')
    expect(ui().text()).not.toContain('Request failed')
    expect(saveButton(wrapper).attributes('disabled')).toBeUndefined()
    expect((input(wrapper).element as HTMLInputElement).value).toBe('Min titel')
  })

  it('saves an explicitly edited default-equivalent value as an override', async () => {
    get.mockResolvedValueOnce(response({ 'page.title': 'En anden titel' }))
    const wrapper = render()
    await flushPromises()
    await input(wrapper).setValue('Om Elektroteknisk forening')
    patch.mockResolvedValueOnce(response({ 'page.title': 'Om Elektroteknisk forening' }))
    await submit(wrapper)
    expect(patch).toHaveBeenCalledWith('/SiteTexts/about', {
      changes: { 'page.title': 'Om Elektroteknisk forening' },
    })
    expect(useSiteTextStore().groups.about.overrides['page.title']).toBe(
      'Om Elektroteknisk forening',
    )
  })

  it('does not save when opened directly by a non-admin and handles unregistered groups', async () => {
    useAuthStore().clearAuth()
    const wrapper = render()
    await flushPromises()
    await input(wrapper).setValue('Uautoriseret')
    await submit(wrapper)
    expect(patch).not.toHaveBeenCalled()
    expect(saveButton(wrapper).attributes('disabled')).toBeDefined()
    await wrapper.setProps({ group: 'unknown' })
    await flushPromises()
    expect(ui().text()).toContain('Tekstgruppen findes ikke.')
    expect(ui().findAll('input')).toHaveLength(0)
    expect(get).toHaveBeenCalledTimes(1)
  })

  it('reacts to login, role loss and logout without unmounting an open draft', async () => {
    useAuthStore().clearAuth()
    const wrapper = mount(SiteTextEditAction, {
      props: { group: 'about' },
      attachTo: document.body,
      global: { stubs: { teleport: false, transition: false } },
    })
    wrappers.push(wrapper)
    expect(wrapper.find('button').exists()).toBe(false)
    const auth = admin()
    await nextTick()
    await wrapper.get('button').trigger('click')
    await flushPromises()
    await input(wrapper).setValue('Behold kladde')
    auth.user!.roles = []
    await nextTick()
    expect(wrapper.find('button').exists()).toBe(false)
    expect(saveButton(wrapper).attributes('disabled')).toBeDefined()
    await submit(wrapper)
    expect(patch).not.toHaveBeenCalled()
    auth.clearAuth()
    await nextTick()
    expect((input(wrapper).element as HTMLInputElement).value).toBe('Behold kladde')
    admin()
    await nextTick()
    expect(saveButton(wrapper).attributes('disabled')).toBeUndefined()
  })

  it('moves focus into the real modal and supports Escape from a field', async () => {
    const wrapper = mount(SiteTextEditAction, {
      props: { group: 'about' },
      attachTo: document.body,
      global: { stubs: { teleport: false, transition: false } },
    })
    wrappers.push(wrapper)
    const action = wrapper.get('button')
    await flushPromises()
    ;(action.element as HTMLButtonElement).focus()
    await action.trigger('click')
    await flushPromises()
    await vi.waitFor(() => expect(document.activeElement?.textContent?.trim()).toBe('Luk'))
    ;(input(wrapper).element as HTMLInputElement).focus()
    await input(wrapper).trigger('keydown', { key: 'Escape' })
    await flushPromises()
    await vi.waitFor(() => expect(ui().find('[role="dialog"]').exists()).toBe(false))
    // Focus restoration and Tab wrapping are checked in Chromium, not jsdom.
  })
})
