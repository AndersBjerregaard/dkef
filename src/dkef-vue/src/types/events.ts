export interface EventDto {
  title: string
  section: string
  address: string
  dateTime: string
  description: string
  signUpDeadline?: string
  thumbnailId?: string
  attachmentIds?: string[]
  attachmentFileNames?: string[]
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
  id: string
  title: string
  section: string
  address: string
  dateTime: string
  description: string
  signUpDeadline?: string | null
  thumbnailUrl: string
  attachmentUrls: string[]
  attachmentFileNames?: string[]
  createdAt: string
}

export interface EventSignUpStatus {
  isSignedUp: boolean
  signedUpAt?: string | null
}

export interface EventSignUpCreateResponse {
  eventId: string
  contactId: string
  signedUpAt: string
}

export interface EventSignUpListItem {
  contactId: string
  name: string
  email: string
  signedUpAt: string
}

export interface EventSignUpsSummary {
  total: number
  collection: EventSignUpListItem[]
}

export interface EventsCollection {
  total: number
  collection: PublishedEvent[]
}
