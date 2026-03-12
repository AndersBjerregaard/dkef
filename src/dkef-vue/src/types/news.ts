export interface NewsDto {
  title: string
  section: string
  author: string
  description: string
  thumbnailId: string
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
}

export interface NewsCollection {
  total: number
  collection: PublishedNews[]
}
