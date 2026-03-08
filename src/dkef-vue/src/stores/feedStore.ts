import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { AxiosResponse } from 'axios'
import apiservice from '@/services/apiservice'
import urlservice from '@/services/urlservice'
import type { FeedItem, FeedResponse } from '@/types/feed'

export const useFeedStore = defineStore('feed', () => {
  const items = ref<FeedItem[]>([])
  const isFetching = ref(false)
  const error = ref<string | null>(null)

  async function fetchFeed(take: number = 9): Promise<void> {
    isFetching.value = true
    error.value = null
    try {
      const response: AxiosResponse<FeedResponse> = await apiservice.get<FeedResponse>(
        urlservice.getFeed(),
        { params: { take } },
      )
      items.value = response.data
    } catch (err: unknown) {
      const errorMessage = `Error attempting to fetch feed: ${err}`
      error.value = errorMessage
      console.error(errorMessage)
    } finally {
      isFetching.value = false
    }
  }

  return { items, isFetching, error, fetchFeed }
})
