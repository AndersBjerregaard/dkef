import type { AttachmentDto } from './attachments'

export interface GeneralAssemblyDto {
  title: string
  section: string
  address: string
  dateTime: string
  description: string
  thumbnailId?: string
  attachments?: AttachmentDto[]
}

export interface PublishedGeneralAssembly {
  id: string
  title: string
  section: string
  address: string
  dateTime: string
  description: string
  thumbnailUrl: string
  createdAt: string
  attachments: AttachmentDto[]
}

export interface GeneralAssemblyCollection {
  total: number
  collection: PublishedGeneralAssembly[]
}
