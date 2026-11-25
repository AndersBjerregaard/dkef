import { defineStore } from 'pinia'
import { reactive, readonly } from 'vue'
import { getSiteImageGroup, getSiteImageSlot, type SiteImageValue } from '@/content/siteImages'
import apiservice from '@/services/apiservice'
import urlservice from '@/services/urlservice'

export const useSiteImageStore = defineStore('siteImages', () => {
  const overrides = reactive<Record<string, Record<string, SiteImageValue>>>({})
  const saving = reactive<Record<string, boolean>>({})
  const loaded = new Set<string>()
  const generations = new Map<string, number>()
  const pending = new Map<string, Promise<void>>()

  function value(group: string, slot: string): SiteImageValue {
    return overrides[group]?.[slot] ?? getSiteImageSlot(group, slot)
  }

  function readValue(data: unknown): SiteImageValue {
    if (!data || typeof data !== 'object') throw new Error('Ugyldigt billedsvar.')
    const { url, alt } = data as Partial<SiteImageValue>
    if (
      typeof url !== 'string' ||
      !/^https?:\/\//.test(url) ||
      typeof alt !== 'string' ||
      !alt.trim() ||
      alt.length > 300
    )
      throw new Error('Ugyldigt billedsvar.')
    return { url, alt }
  }

  function loadGroup(group: string, force = false): Promise<void> {
    const definition = getSiteImageGroup(group)
    if (!definition) return Promise.reject(new Error('Billedgruppen findes ikke.'))
    if (!force && pending.has(group)) return pending.get(group)!
    if (!force && loaded.has(group)) return Promise.resolve()
    const generation = (generations.get(group) ?? 0) + 1
    generations.set(group, generation)
    const request = Promise.resolve().then(async () => {
      try {
        const { data } = await apiservice.get<unknown>(urlservice.getSiteImages(group), {
          skipAuth: true,
          withCredentials: false,
        })
        const response = data as { group?: unknown; overrides?: unknown } | null
        if (
          response?.group !== group ||
          !response.overrides ||
          typeof response.overrides !== 'object' ||
          Array.isArray(response.overrides)
        )
          throw new Error('Ugyldigt billedsvar.')
        const entries = Object.entries(response.overrides)
        const parsed = Object.fromEntries(
          entries.map(([slot, image]) => {
            getSiteImageSlot(group, slot)
            return [slot, readValue(image)]
          }),
        )
        if (generations.get(group) !== generation) {
          if (force) throw new Error('Billederne blev ændret under hentningen. Prøv igen.')
          return
        }
        overrides[group] = parsed
        loaded.add(group)
      } finally {
        if (pending.get(group) === request) pending.delete(group)
      }
    })
    pending.set(group, request)
    return request
  }

  async function change(group: string, slot: string, file?: File, alt?: string) {
    getSiteImageSlot(group, slot)
    if (saving[group]) throw new Error('Et billede er allerede ved at blive gemt.')
    saving[group] = true
    try {
      let replacement: SiteImageValue | undefined
      if (file) {
        const form = new FormData()
        form.append('file', file)
        form.append('alt', alt ?? '')
        const response = await apiservice.put<unknown>(urlservice.siteImage(group, slot), form, {
          timeout: 60000,
        })
        replacement = readValue(response.data)
      } else {
        await apiservice.delete(urlservice.siteImage(group, slot))
      }
      generations.set(group, (generations.get(group) ?? 0) + 1)
      pending.delete(group)
      overrides[group] ??= {}
      if (replacement) overrides[group][slot] = replacement
      else delete overrides[group][slot]
    } finally {
      saving[group] = false
    }
  }

  return { overrides: readonly(overrides), saving: readonly(saving), value, loadGroup, change }
})
