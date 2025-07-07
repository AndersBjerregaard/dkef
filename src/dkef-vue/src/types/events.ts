export interface EventDto {
  title: string,
  section: string,
  address: string,
  dateTime: string,
  description: string,
  thumbnailId: string
}

/**
 * @prop id: string
 * @prop title: string
 * @prop section: string
 * @prop address: string
 * @prop dateTime: string
 * @prop description: string
 * @prop thumbnailUrl: string
 * @prop createdAt: string
 */
export interface PublishedEvent {
  id: string,
  title: string,
  section: string,
  address: string,
  dateTime: string,
  description: string,
  thumbnailUrl: string,
  createdAt: string
}

export interface EventsCollection {
  total: number,
  collection: PublishedEvent[]
}
