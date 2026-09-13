import { defineStore } from 'pinia'
import apiservice from '@/services/apiservice'
import type {
  EventDto,
  EventSignUpCreateResponse,
  EventSignUpStatus,
  EventSignUpsSummary,
  EventsCollection,
  PublishedEvent,
} from '@/types/events'
import type { NexiCheckoutSessionDto } from '@/types/payment'
import urlservice from '@/services/urlservice'
import type { AxiosResponse } from 'axios'

interface EventState {
  events: { [id: string]: PublishedEvent }
  isFetching: boolean
  error: string | null
}

interface EventGetters {
  [key: string]:
    | ((state: EventState) => unknown)
    | ((state: EventState) => (id: string) => PublishedEvent | undefined)
  getEventById: (state: EventState) => (id: string) => PublishedEvent | undefined
}

interface EventActions {
  fetchLatestEvents: () => Promise<PublishedEvent[]>
  fetchEvent: (id: string) => Promise<PublishedEvent | undefined>
  updateEvent: (id: string, dto: EventDto) => Promise<void>
  deleteEventItem: (id: string) => Promise<void>
  signUpForEvent: (id: string) => Promise<EventSignUpCreateResponse>
  createPaidEventCheckoutSession: (id: string) => Promise<NexiCheckoutSessionDto>
  confirmPaidEventPayment: (id: string, paymentId: string) => Promise<EventSignUpCreateResponse>
  fetchMySignUpStatus: (id: string) => Promise<EventSignUpStatus>
  fetchEventSignUps: (id: string) => Promise<EventSignUpsSummary>
}

export const useEventStore = defineStore<'event', EventState, EventGetters, EventActions>('event', {
  state: () => ({
    events: {},
    isFetching: false,
    error: null,
  }),
  getters: {
    getEventById: (state: EventState) => (id: string) => state.events[id],
  },
  actions: {
    async fetchLatestEvents(): Promise<PublishedEvent[]> {
      this.isFetching = true
      this.error = null
      let result: PublishedEvent[] = []
      try {
        const response: AxiosResponse<EventsCollection, unknown> =
          await apiservice.get<EventsCollection>(urlservice.getEvents(), {
            params: {
              take: 3,
              orderBy: 'DateTime',
              sortOrder: 'asc',
              timeframe: 'upcoming',
            },
          })
        const events: PublishedEvent[] = response.data.collection
        events.forEach((event: PublishedEvent) => {
          this.events[event.id] = event
        })
        result = events
      } catch (error: unknown) {
        const errorMessage = `Error attempting to fetch latest events ${error}`
        this.error = errorMessage
        console.error(errorMessage)
      } finally {
        this.isFetching = false
      }
      return result
    },
    async fetchEvent(id: string): Promise<PublishedEvent | undefined> {
      if (this.events[id]) {
        return this.events[id]
      }
      this.isFetching = true
      this.error = null
      try {
        const response: AxiosResponse<PublishedEvent, unknown> =
          await apiservice.get<PublishedEvent>(urlservice.getEvent(id))
        const event: PublishedEvent = response.data
        this.events[id] = event
        return event
      } catch (error) {
        const errorMessage = `Error attempting to fetch event ${id} ${error}`
        this.error = errorMessage
        console.error(errorMessage)
      } finally {
        this.isFetching = false
      }
      return undefined
    },
    async updateEvent(id: string, dto: EventDto): Promise<void> {
      this.isFetching = true
      this.error = null
      try {
        const response: AxiosResponse<PublishedEvent> = await apiservice.put<PublishedEvent>(
          urlservice.updateEvent(id),
          dto,
        )
        this.events[id] = response.data
      } catch (error: unknown) {
        const errorMessage = `Error attempting to update event ${id}: ${error}`
        this.error = errorMessage
        console.error(errorMessage)
        throw error
      } finally {
        this.isFetching = false
      }
    },
    async deleteEventItem(id: string): Promise<void> {
      this.isFetching = true
      try {
        await apiservice.delete(urlservice.deleteEvent(id))
        delete this.events[id]
      } catch (error) {
        const errorMessage = `Error attempting to delete event ${id}: ${error}`
        console.error(errorMessage)
        throw error
      } finally {
        this.isFetching = false
      }
    },
    async signUpForEvent(id: string): Promise<EventSignUpCreateResponse> {
      this.error = null
      const response: AxiosResponse<EventSignUpCreateResponse> =
        await apiservice.post<EventSignUpCreateResponse>(urlservice.postEventSignUp(id), {})
      return response.data
    },
    async createPaidEventCheckoutSession(id: string): Promise<NexiCheckoutSessionDto> {
      this.error = null
      const response: AxiosResponse<NexiCheckoutSessionDto> =
        await apiservice.post<NexiCheckoutSessionDto>(urlservice.getNexiEventSession(id), {})
      return response.data
    },
    async confirmPaidEventPayment(id: string, paymentId: string): Promise<EventSignUpCreateResponse> {
      this.error = null
      const response: AxiosResponse<EventSignUpCreateResponse> =
        await apiservice.post<EventSignUpCreateResponse>(urlservice.postNexiEventConfirm(id), {
          paymentId,
        })
      return response.data
    },
    async fetchMySignUpStatus(id: string): Promise<EventSignUpStatus> {
      this.error = null
      const response: AxiosResponse<EventSignUpStatus> = await apiservice.get<EventSignUpStatus>(
        urlservice.getMyEventSignUpStatus(id),
      )
      return response.data
    },
    async fetchEventSignUps(id: string): Promise<EventSignUpsSummary> {
      this.error = null
      const response: AxiosResponse<EventSignUpsSummary> = await apiservice.get<EventSignUpsSummary>(
        urlservice.getEventSignUps(id),
      )
      return response.data
    },
  },
})
