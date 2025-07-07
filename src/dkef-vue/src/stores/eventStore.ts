import { defineStore } from "pinia";
import apiservice from "@/services/apiservice";
import type { EventsCollection, PublishedEvent } from "@/types/events";
import urlservice from "@/services/urlservice";
import type { AxiosResponse } from "axios";

interface EventState {
  events: { [id: string]: PublishedEvent };
  isFetching: boolean;
  error: string | null;
}

interface EventGetters {
  [key: string]: ((state: EventState) => any) | ((state: EventState) => (id: string) => PublishedEvent | undefined);
  getEventById: (state: EventState) => (id: string) => PublishedEvent | undefined;
}

interface EventActions {
  fetchLatestEvents: () => Promise<PublishedEvent[]>
  fetchEvent: (id: string) => Promise<PublishedEvent | undefined>
}

export const useEventStore = defineStore<'event', EventState, EventGetters, EventActions>('event', {
  state: () => ({
    events: {},
    isFetching: false,
    error: null,
  }),
  getters: {
    getEventById: (state: EventState) => (id: string) => state.events[id]
  },
  actions: {
    async fetchLatestEvents(): Promise<PublishedEvent[]> {
      this.isFetching = true;
      this.error = null;
      let result: PublishedEvent[] = [];
      try {
        const response: AxiosResponse<EventsCollection, any> = await apiservice.get<EventsCollection>(urlservice.getEvents(), {
          params: {
            take: 3,
            orderBy: 'dateTime',
            order: 'desc'
          }
        });
        const events: PublishedEvent[] = response.data.collection;
        events.forEach((event: PublishedEvent) => {
          this.events[event.id] = event;
        });
        result = events;
      } catch (error: any) {
        const errorMessage = `Error attempting to fetch latest events ${error}`;
        this.error = errorMessage;
        console.error(errorMessage);
      } finally {
        this.isFetching = false;
      }
      return result;
    },
    async fetchEvent(id: string): Promise<PublishedEvent | undefined> {
      if (this.events[id]) {
        return this.events[id];
      }
      this.isFetching = true;
      this.error = null;
      try {
        const response: AxiosResponse<PublishedEvent, any> = await apiservice.get<PublishedEvent>(urlservice.getEvent(id));
        const event: PublishedEvent = response.data;
        this.events[id] = event;
        return event;
      } catch (error) {
        const errorMessage = `Error attempting to fetch event ${id} ${error}`;
        this.error = errorMessage;
        console.error(errorMessage);
      } finally {
        this.isFetching = false;
      }
      return undefined;
    }
  }
})
