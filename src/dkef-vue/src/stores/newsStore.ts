import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { AxiosResponse } from 'axios'
import apiservice from '@/services/apiservice'
import urlservice from '@/services/urlservice'
import type { NewsCollection, PublishedNews } from '@/types/news'

export const useNewsStore = defineStore('news', () => {
  const news = ref<Record<string, PublishedNews>>({})
  const isFetching = ref(false)
  const error = ref<string | null>(null)

  function getNewsById(id: string): PublishedNews | undefined {
    return news.value[id]
  }

  async function fetchLatestNews(): Promise<PublishedNews[]> {
    isFetching.value = true
    error.value = null
    let result: PublishedNews[] = []
    try {
      const response: AxiosResponse<NewsCollection> = await apiservice.get<NewsCollection>(
        urlservice.getNews(),
        {
          params: {
            take: 3,
            orderBy: 'publishedAt',
            order: 'desc',
          },
        },
      )
      const items: PublishedNews[] = response.data.collection
      items.forEach((item: PublishedNews) => {
        news.value[item.id] = item
      })
      result = items
    } catch (err: unknown) {
      const errorMessage = `Error attempting to fetch latest news: ${err}`
      error.value = errorMessage
      console.error(errorMessage)
    } finally {
      isFetching.value = false
    }
    return result
  }

  async function fetchNewsItem(id: string): Promise<PublishedNews | undefined> {
    if (news.value[id]) {
      return news.value[id]
    }
    isFetching.value = true
    error.value = null
    try {
      const response: AxiosResponse<PublishedNews> = await apiservice.get<PublishedNews>(
        urlservice.getNewsItem(id),
      )
      const item: PublishedNews = response.data
      news.value[item.id] = item
      return item
    } catch (err: unknown) {
      const errorMessage = `Error attempting to fetch news item ${id}: ${err}`
      error.value = errorMessage
      console.error(errorMessage)
    } finally {
      isFetching.value = false
    }
    return undefined
  }

  return { news, isFetching, error, getNewsById, fetchLatestNews, fetchNewsItem }
})
