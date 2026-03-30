import type { AttachmentDto } from './attachments'

export interface NewsDto {
  title: string
  section: string
  author: string
  description: string
  thumbnailId?: string
  attachments?: AttachmentDto[]
}

export interface PublishedNews {
  id: string
  title: string
  section: string
  author: string
  description: string
  thumbnailUrl: string
  publishedAt: string
  createdAt: string
  attachments: AttachmentDto[]
}

export interface NewsCollection {
  total: number
  collection: PublishedNews[]
}
