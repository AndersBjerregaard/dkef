import axios from 'axios'

// Keep aligned with automation/k8s/minio/ingress.yaml (NGINX "20m").
export const MAX_STORAGE_UPLOAD_BYTES = 20 * 1024 * 1024
export const STORAGE_UPLOAD_HINT = 'Hver fil må højst fylde 20 MiB (20.971.520 bytes).'

export function getStorageFileError(file: File): string | null {
  if (file.size === 0) return `"${file.name}": Filen er tom (0 bytes).`
  if (file.size > MAX_STORAGE_UPLOAD_BYTES) {
    return `"${file.name}": Filen er for stor. ${STORAGE_UPLOAD_HINT}`
  }
  return null
}

export async function uploadFile(url: string, file: File): Promise<void> {
  const validationError = getStorageFileError(file)
  if (validationError) throw new Error(validationError)
  try {
    // Do not use the API client: storage uses a signed URL, not a Bearer token.
    await axios.create().put(url, file, { headers: { 'Content-Type': file.type } })
  } catch (error: unknown) {
    if (axios.isAxiosError(error) && error.response?.status === 413) {
      throw new Error(`"${file.name}": Serveren afviste filen som for stor. ${STORAGE_UPLOAD_HINT}`)
    }
    // Avoid propagating Axios configuration containing the signed URL.
    throw new Error(`"${file.name}": Filen kunne ikke uploades. Prøv igen.`)
  }
}
