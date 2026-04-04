export interface FeedItem {
  id: string
  kind: 'event' | 'news' | 'general-assembly'
  title: string
  section: string
  description: string
  thumbnailUrl: string
  createdAt: string
  dateTime?: string
  // Event + GeneralAssembly specific
  address?: string
}

export interface FeedCollection {
  total: number
  collection: FeedItem[]
}

export type FeedResponse = FeedCollection
