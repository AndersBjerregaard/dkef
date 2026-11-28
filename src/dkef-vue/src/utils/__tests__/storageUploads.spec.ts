import { beforeEach, describe, expect, it, vi } from 'vitest'
import axios from 'axios'
import { readFileSync } from 'node:fs'
import { resolve } from 'node:path'
import { getStorageFileError, MAX_STORAGE_UPLOAD_BYTES, uploadFile } from '../storageUploads'

vi.mock('axios', () => ({
  default: { create: vi.fn(), isAxiosError: vi.fn() },
}))

function file(size: number): File {
  const value = new File(['pdf'], 'document.pdf', { type: 'application/pdf' })
  Object.defineProperty(value, 'size', { value: size })
  return value
}

describe('direct storage uploads', () => {
  const put = vi.fn()
  beforeEach(() => {
    vi.resetAllMocks()
    vi.mocked(axios.create).mockReturnValue({ put } as unknown as ReturnType<typeof axios.create>)
  })

  it('matches the storage ingress limit', () => {
    const manifest = readFileSync(
      resolve(__dirname, '../../../../../automation/k8s/minio/ingress.yaml'),
      'utf8',
    )
    const limit = manifest.match(/nginx\.ingress\.kubernetes\.io\/proxy-body-size: "(\d+)m"/)
    expect(limit).not.toBeNull()
    expect(Number(limit![1]) * 1024 * 1024).toBe(MAX_STORAGE_UPLOAD_BYTES)
  })

  it.each([1, 1187289, MAX_STORAGE_UPLOAD_BYTES])('accepts %i bytes', (size) => {
    expect(getStorageFileError(file(size))).toBeNull()
  })

  it('rejects empty and oversized files with Danish feedback', () => {
    expect(getStorageFileError(file(0))).toContain('Filen er tom')
    expect(getStorageFileError(file(MAX_STORAGE_UPLOAD_BYTES + 1))).toContain('20 MiB')
  })

  it('does not send invalid files', async () => {
    await expect(
      uploadFile('https://storage.invalid/signed', file(MAX_STORAGE_UPLOAD_BYTES + 1)),
    ).rejects.toThrow('Filen er for stor')
    expect(axios.create).not.toHaveBeenCalled()
  })

  it('uploads a raw file with its content type', async () => {
    const pdf = file(1187289)
    await uploadFile('https://storage.invalid/signed', pdf)
    expect(put).toHaveBeenCalledWith('https://storage.invalid/signed', pdf, {
      headers: { 'Content-Type': 'application/pdf' },
    })
  })

  it('translates server 413 responses without leaking signed URLs', async () => {
    put.mockRejectedValue({ response: { status: 413 }, config: { url: 'secret-signature' } })
    vi.mocked(axios.isAxiosError).mockReturnValue(true)
    await expect(uploadFile('https://storage.invalid/signed', file(1187289))).rejects.toThrow(
      'Serveren afviste filen som for stor',
    )
  })

  it('reports other upload failures without exposing transport details', async () => {
    put.mockRejectedValue(new Error('secret-signature'))
    await expect(uploadFile('https://storage.invalid/signed', file(1187289))).rejects.toThrow(
      'Filen kunne ikke uploades. Prøv igen.',
    )
  })
})
