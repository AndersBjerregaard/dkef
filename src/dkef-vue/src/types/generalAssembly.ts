export interface GeneralAssemblyDto {
  title: string
  section: string
  address: string
  dateTime: string
  description: string
  thumbnailId?: string
  attachmentIds?: string[]
}

export interface PublishedGeneralAssembly {
  id: string
  title: string
  section: string
  address: string
  dateTime: string
  description: string
  thumbnailUrl: string
  attachmentUrls: string[]
  createdAt: string
}

export interface GeneralAssemblyCollection {
  total: number
  collection: PublishedGeneralAssembly[]
}
