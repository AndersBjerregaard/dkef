import { mount, flushPromises } from '@vue/test-utils'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import AttachmentUploader from '../AttachmentUploader.vue'
import apiservice from '@/services/apiservice'
import { MAX_STORAGE_UPLOAD_BYTES, uploadFile } from '@/utils/storageUploads'

vi.mock('@/services/apiservice', () => ({ default: { get: vi.fn() } }))
vi.mock('@/services/urlservice', () => ({
  default: { getAttachmentPresignedUrl: (id: string) => `/bucket/attachments/${id}` },
}))
vi.mock('@/utils/storageUploads', async (importOriginal) => ({
  ...(await importOriginal<typeof import('@/utils/storageUploads')>()),
  uploadFile: vi.fn(),
}))

function file(name: string, size: number): File {
  const value = new File(['pdf'], name, { type: 'application/pdf' })
  Object.defineProperty(value, 'size', { value: size })
  return value
}

function setup() {
  return mount(AttachmentUploader, {
    props: { attachments: [], isLoading: false, contentType: 'events' },
  })
}

async function select(wrapper: ReturnType<typeof setup>, files: File[]) {
  const input = wrapper.get('input')
  Object.defineProperty(input.element, 'files', { value: files, configurable: true })
  await input.trigger('change')
}

describe('AttachmentUploader', () => {
  beforeEach(() => {
    vi.resetAllMocks()
    vi.mocked(apiservice.get).mockResolvedValue({ data: 'https://storage.invalid/signed' } as never)
    vi.mocked(uploadFile).mockResolvedValue(undefined)
  })

  it('shows the limit and excludes oversized/empty files from a mixed selection', async () => {
    const wrapper = setup()
    await select(wrapper, [
      file('large.pdf', MAX_STORAGE_UPLOAD_BYTES + 1),
      file('empty.pdf', 0),
      file('valid.pdf', 1187289),
    ])
    expect(wrapper.get('[role="alert"]').text()).toContain('large.pdf')
    expect(wrapper.text()).toContain('20 MiB')
    const result = await wrapper.vm.processNewAttachments()
    expect(result).toHaveLength(1)
    expect(result[0].fileName).toBe('valid.pdf')
    expect(apiservice.get).toHaveBeenCalledTimes(1)
  })

  it('accepts exactly the limit and clears selection errors after a valid selection', async () => {
    const wrapper = setup()
    await select(wrapper, [file('large.pdf', MAX_STORAGE_UPLOAD_BYTES + 1)])
    await select(wrapper, [file('boundary.pdf', MAX_STORAGE_UPLOAD_BYTES)])
    expect(wrapper.find('[role="alert"]').exists()).toBe(false)
    expect(await wrapper.vm.processNewAttachments()).toHaveLength(1)
  })

  it('blocks saving on upload failure, preserves completed uploads and retries pending files', async () => {
    const wrapper = setup()
    await select(wrapper, [file('first.pdf', 1), file('second.pdf', 1)])
    vi.mocked(uploadFile)
      .mockResolvedValueOnce(undefined)
      .mockRejectedValueOnce(new Error('Serveren afviste filen som for stor. 20 MiB.'))
    await expect(wrapper.vm.processNewAttachments()).rejects.toThrow('20 MiB')
    await flushPromises()
    expect(wrapper.get('[role="alert"]').text()).toContain('20 MiB')
    const completed = wrapper.emitted('update:attachments')![0][0]
    await wrapper.setProps({ attachments: completed as never })
    expect(wrapper.get('[role="alert"]').text()).toContain('20 MiB')
    const retry = await wrapper.vm.processNewAttachments()
    expect(retry.map((attachment) => attachment.fileName)).toEqual(['second.pdf'])
    expect(apiservice.get).toHaveBeenCalledTimes(3)
  })
})
