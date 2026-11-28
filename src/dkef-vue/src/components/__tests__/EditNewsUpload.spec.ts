import { mount, flushPromises } from '@vue/test-utils'
import { createPinia } from 'pinia'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import EditNewsModal from '../EditNewsModal.vue'
import apiservice from '@/services/apiservice'
import { MAX_STORAGE_UPLOAD_BYTES, uploadFile } from '@/utils/storageUploads'

vi.mock('@/services/apiservice', () => ({ default: { get: vi.fn(), put: vi.fn() } }))
vi.mock('@/services/urlservice', () => ({
  default: {
    getNewsPresignedUrl: () => '/signed',
    getAttachmentPresignedUrl: () => '/signed',
    updateNews: () => '/news/test',
  },
}))
vi.mock('@/utils/storageUploads', async (importOriginal) => ({
  ...(await importOriginal<typeof import('@/utils/storageUploads')>()),
  uploadFile: vi.fn(),
}))

function setup() {
  return mount(EditNewsModal, {
    props: {
      isOpen: true,
      news: {
        id: 'test',
        title: 'Nyhed',
        section: '',
        description: 'Beskrivelse',
        thumbnailUrl: '',
        attachmentUrls: [],
        dateTime: '',
        createdAt: '',
      },
    },
    global: { plugins: [createPinia()], stubs: { BaseModal: { template: '<div><slot /></div>' } } },
  })
}

describe('news upload feedback', () => {
  beforeEach(() => {
    vi.resetAllMocks()
    vi.mocked(apiservice.get).mockResolvedValue({ data: 'https://storage.invalid/signed' } as never)
  })

  it('blocks oversized thumbnails before requesting an upload URL', async () => {
    const wrapper = setup()
    const file = new File(['image'], 'large.png', { type: 'image/png' })
    Object.defineProperty(file, 'size', { value: MAX_STORAGE_UPLOAD_BYTES + 1 })
    const input = wrapper.get('input[type="file"]')
    Object.defineProperty(input.element, 'files', { value: [file] })
    await input.trigger('change')
    expect(wrapper.get('[role="alert"]').text()).toContain('20 MiB')
    await wrapper.get('form').trigger('submit')
    await flushPromises()
    expect(apiservice.get).not.toHaveBeenCalled()
    expect(apiservice.put).not.toHaveBeenCalled()
    expect(wrapper.emitted('close')).toBeUndefined()
  })

  it('keeps the modal open and does not save after an attachment upload fails', async () => {
    const wrapper = setup()
    const input = wrapper.get('#attachment_files')
    Object.defineProperty(input.element, 'files', { value: [new File(['pdf'], 'document.pdf')] })
    await input.trigger('change')
    vi.mocked(uploadFile).mockRejectedValue(
      new Error('Serveren afviste filen som for stor. 20 MiB.'),
    )
    const errorLog = vi.spyOn(console, 'error').mockImplementation(() => {})
    try {
      await wrapper.get('form').trigger('submit')
      await flushPromises()
      expect(wrapper.text()).toContain('Serveren afviste filen som for stor')
      expect(apiservice.put).not.toHaveBeenCalled()
      expect(wrapper.emitted('close')).toBeUndefined()
    } finally {
      errorLog.mockRestore()
    }
  })
})
