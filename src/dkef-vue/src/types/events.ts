export interface EventDto {
  title: string,
  section: string,
  address: string,
  dateTime: string,
  description: string,
  thumbnailId: string
}

export interface PublishedEvent {
  id: string,
  title: string,
  section: string,
  address: string,
  dateTime: string,
  description: string,
  thumbnailUrl: string
}
