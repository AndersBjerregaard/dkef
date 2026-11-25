import { beforeEach, describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import SiteImageEditAction from '../SiteImageEditAction.vue'
import SiteTextEditAction from '../SiteTextEditAction.vue'
import { useAuthStore } from '@/stores/authStore'

beforeEach(() => {
  setActivePinia(createPinia())
})

describe.each([SiteImageEditAction, SiteTextEditAction])('site edit action', (component) => {
  it('shows a pointer cursor for admins and opens its editor', async () => {
    const auth = useAuthStore()
    auth.accessToken = 'test-token'
    auth.user = { email: 'admin@example.dk', name: 'Admin', roles: ['Admin'] }
    const wrapper = mount(component, {
      props: { group: 'home' },
      global: { stubs: { SiteImageEditorModal: true, SiteTextEditorModal: true } },
    })

    expect(wrapper.get('button').classes()).toContain('cursor-pointer')
    await wrapper.get('button').trigger('click')
    expect(
      wrapper
        .findComponent({
          name: component === SiteImageEditAction ? 'SiteImageEditorModal' : 'SiteTextEditorModal',
        })
        .props('isOpen'),
    ).toBe(true)
    wrapper.unmount()
  })
})
