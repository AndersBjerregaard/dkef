export interface NewsDto {
  title: string
  section: string
  dateTime: string
  description: string
  thumbnailId?: string
}

export interface PublishedNews {
  id: string
  title: string
  section: string
  description: string
  thumbnailUrl: string
  dateTime: string
  createdAt: string
}

export interface NewsCollection {
  total: number
  collection: PublishedNews[]
}
