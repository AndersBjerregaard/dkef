import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { AxiosResponse } from 'axios'
import apiservice from '@/services/apiservice'
import urlservice from '@/services/urlservice'
import type { GeneralAssemblyCollection, PublishedGeneralAssembly } from '@/types/generalAssembly'

export const useGeneralAssemblyStore = defineStore('generalAssembly', () => {
  const assemblies = ref<Record<string, PublishedGeneralAssembly>>({})
  const isFetching = ref(false)
  const error = ref<string | null>(null)

  function getGeneralAssemblyById(id: string): PublishedGeneralAssembly | undefined {
    return assemblies.value[id]
  }

  async function fetchLatestGeneralAssemblies(): Promise<PublishedGeneralAssembly[]> {
    isFetching.value = true
    error.value = null
    let result: PublishedGeneralAssembly[] = []
    try {
      const response: AxiosResponse<GeneralAssemblyCollection> =
        await apiservice.get<GeneralAssemblyCollection>(urlservice.getGeneralAssemblies(), {
          params: {
            take: 3,
            orderBy: 'dateTime',
            order: 'desc',
          },
        })
      const items: PublishedGeneralAssembly[] = response.data.collection
      items.forEach((item: PublishedGeneralAssembly) => {
        assemblies.value[item.id] = item
      })
      result = items
    } catch (err: unknown) {
      const errorMessage = `Error attempting to fetch latest general assemblies: ${err}`
      error.value = errorMessage
      console.error(errorMessage)
    } finally {
      isFetching.value = false
    }
    return result
  }

  async function fetchGeneralAssembly(id: string): Promise<PublishedGeneralAssembly | undefined> {
    if (assemblies.value[id]) {
      return assemblies.value[id]
    }
    isFetching.value = true
    error.value = null
    try {
      const response: AxiosResponse<PublishedGeneralAssembly> =
        await apiservice.get<PublishedGeneralAssembly>(urlservice.getGeneralAssembly(id))
      const item: PublishedGeneralAssembly = response.data
      assemblies.value[item.id] = item
      return item
    } catch (err: unknown) {
      const errorMessage = `Error attempting to fetch general assembly ${id}: ${err}`
      error.value = errorMessage
      console.error(errorMessage)
    } finally {
      isFetching.value = false
    }
    return undefined
  }

  return {
    assemblies,
    isFetching,
    error,
    getGeneralAssemblyById,
    fetchLatestGeneralAssemblies,
    fetchGeneralAssembly,
  }
})
