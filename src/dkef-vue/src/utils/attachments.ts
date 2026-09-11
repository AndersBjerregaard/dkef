import type { AttachmentItem } from '@/types/attachments'

export function getAttachmentIdFromUrl(url: string): string {
  try {
    const parsedUrl = new URL(url)
    const parts = parsedUrl.pathname.split('/').filter(Boolean)
    return parts.length > 0 ? parts[parts.length - 1] : url
  } catch {
    const parts = url.split('/').filter(Boolean)
    return parts.length > 0 ? parts[parts.length - 1] : url
  }
}

export function toAttachmentItems(
  attachmentUrls: string[],
  attachmentFileNames?: string[],
): AttachmentItem[] {
  return attachmentUrls.map((url, index) => ({
    id: getAttachmentIdFromUrl(url),
    fileName: attachmentFileNames?.[index] ?? '',
  }))
}
