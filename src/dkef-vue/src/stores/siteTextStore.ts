import { defineStore } from 'pinia'
import { computed, reactive, readonly } from 'vue'
import {
  getSiteTextDefaults,
  getSiteTextFields,
  getSiteTextGroup,
  siteTextRegistry,
} from '@/content/siteTexts'
import apiservice from '@/services/apiservice'
import urlservice from '@/services/urlservice'
import type { SiteTextPatchRequest } from '@/types/siteTexts'

type LoadStatus = 'idle' | 'loading' | 'loaded' | 'error'
type SaveStatus = 'idle' | 'saving' | 'saved' | 'error'
type Values = Readonly<Record<string, string>>

interface GroupState {
  overrides: Record<string, string>
  loadStatus: LoadStatus
  loadError: unknown
  saveStatus: SaveStatus
  saveError: unknown
  editorReady: boolean
}

export const useSiteTextStore = defineStore('siteTexts', () => {
  const groups = reactive<Record<string, GroupState>>({})
  // Promises and generations are in-memory coordination, not persisted state.
  const pendingLoads = new Map<string, Promise<Values>>()
  const generations = new Map<string, number>()
  for (const group of siteTextRegistry.groups) {
    groups[group.id] = {
      overrides: {},
      loadStatus: 'idle',
      loadError: null,
      saveStatus: 'idle',
      saveError: null,
      editorReady: false,
    }
    generations.set(group.id, 0)
  }

  function definition(group: string) {
    const result = getSiteTextGroup(group)
    if (!result) throw new Error('Tekstgruppen findes ikke.')
    return result
  }

  const texts = computed<Readonly<Record<string, Values>>>(() =>
    Object.fromEntries(
      siteTextRegistry.groups.map((group) => [
        group.id,
        Object.freeze({
          ...getSiteTextDefaults(group),
          ...groups[group.id].overrides,
        }),
      ]),
    ),
  )

  /** Detached working copy; callers must retain their original snapshot for changed-key saves. */
  function createDraft(group: string): Record<string, string> {
    definition(group)
    return { ...texts.value[group] }
  }

  function readOverrides(group: string, data: unknown): Record<string, string> {
    const fields = getSiteTextFields(definition(group))
    if (!data || typeof data !== 'object' || Array.isArray(data)) throw invalidResponse()
    const response = data as Record<string, unknown>
    if (
      response.group !== group ||
      !response.overrides ||
      typeof response.overrides !== 'object' ||
      Array.isArray(response.overrides)
    )
      throw invalidResponse()
    const entries = Object.entries(response.overrides)
    if (
      entries.some(
        ([key, value]) => typeof value !== 'string' || !fields.some((field) => field.key === key),
      )
    ) {
      throw invalidResponse()
    }
    return Object.fromEntries(entries) as Record<string, string>
  }

  function invalidResponse() {
    return new Error('API’et returnerede et ugyldigt svar for tekstgruppen.')
  }

  function fetchGroup(group: string, force: boolean, editor = false): Promise<Values> {
    definition(group)
    const state = groups[group]
    const pending = pendingLoads.get(group)
    if (!force && pending) return pending
    if (!force && state.loadStatus === 'loaded') return Promise.resolve(createDraft(group))
    const generation = (generations.get(group) ?? 0) + 1
    generations.set(group, generation)
    state.loadStatus = 'loading'
    state.loadError = null
    state.editorReady = false
    // Defer URL construction too: unavailable mock-mode endpoints are rejected loads.
    const request = Promise.resolve().then(async () => {
      try {
        const response = await apiservice.get<unknown>(urlservice.getSiteTexts(group), {
          skipAuth: true,
          withCredentials: false,
        })
        const overrides = readOverrides(group, response.data)
        if (editor && generations.get(group) !== generation) {
          throw new Error('Teksthentningen blev erstattet. Åbn editoren igen.')
        }
        if (generations.get(group) === generation) {
          state.overrides = overrides
          state.loadStatus = 'loaded'
          state.editorReady = editor
        }
        return createDraft(group)
      } catch (error) {
        if (generations.get(group) === generation) {
          state.loadStatus = 'error'
          state.loadError = error
          state.editorReady = false
        }
        throw error
      } finally {
        if (pendingLoads.get(group) === request) pendingLoads.delete(group)
      }
    })
    pendingLoads.set(group, request)
    return request
  }

  /** Always re-read before editing, even if a public display load is cached or pending. */
  function refreshForEditor(group: string): Promise<Values> {
    return fetchGroup(group, true, true)
  }

  function loadGroup(group: string): Promise<Values> {
    return fetchGroup(group, false)
  }

  function refreshGroup(group: string): Promise<Values> {
    return fetchGroup(group, true)
  }

  async function saveGroup(group: string, draft: Values, original: Values): Promise<void> {
    const fields = getSiteTextFields(definition(group))
    const state = groups[group]
    if (state.saveStatus === 'saving')
      throw new Error('Tekstgruppen er allerede ved at blive gemt.')
    if (!state.editorReady || state.loadStatus !== 'loaded') {
      throw new Error('Hent teksterne igen, før du gemmer.')
    }
    if (
      Object.entries(draft).some(
        ([key, value]) => typeof value !== 'string' || !fields.some((field) => field.key === key),
      )
    ) {
      throw new Error('Tekstændringerne indeholder et ugyldigt felt.')
    }
    const changes = Object.fromEntries(
      Object.entries(draft).filter(([key, value]) => value !== original[key]),
    )
    if (!Object.keys(changes).length) return
    const request: SiteTextPatchRequest = { changes }
    state.saveStatus = 'saving'
    state.saveError = null
    try {
      const response = await apiservice.patch<unknown>(urlservice.patchSiteTexts(group), request)
      const overrides = readOverrides(group, response.data)
      generations.set(group, (generations.get(group) ?? 0) + 1)
      // Do not let an earlier GET, including an editor refresh, undo this commit.
      pendingLoads.delete(group)
      state.overrides = overrides
      state.loadStatus = 'loaded'
      state.loadError = null
      state.saveStatus = 'saved'
    } catch (error) {
      state.saveStatus = 'error'
      state.saveError = error
      throw error
    }
  }

  // No persist option: the app-wide plugin is opt-in and must not cache published text.
  return {
    groups: readonly(groups),
    texts,
    createDraft,
    loadGroup,
    refreshGroup,
    refreshForEditor,
    saveGroup,
  }
})
