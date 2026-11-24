import document from './siteTextDefinitions.json'
import { hasSiteTextContent, validateSiteTextValue } from './siteTextRules'
import type {
  SiteTextFieldDefinition,
  SiteTextGroupDefinition,
  SiteTextRegistryDefinition,
} from '@/types/siteTexts'

const identifier = /^[a-z][a-z0-9]*(?:[.-][a-z0-9]+)*$/u

/** Validate unknown JSON, not just its TypeScript shape, then freeze all nested definitions. */
export function createSiteTextRegistry(document: unknown): SiteTextRegistryDefinition {
  function object(value: unknown, keys: string[]): Record<string, unknown> {
    if (!value || typeof value !== 'object' || Array.isArray(value)) fail('expected object')
    const result = value as Record<string, unknown>
    if (
      Object.keys(result).length !== keys.length ||
      !keys.every((key) => Object.prototype.hasOwnProperty.call(result, key))
    )
      fail('missing or unknown member')
    return result
  }
  function fail(reason: string): never {
    throw new Error(`Invalid site-text registry: ${reason}`)
  }
  function text(value: unknown): value is string {
    return typeof value === 'string' && hasSiteTextContent(value)
  }
  function ordered(values: unknown, idKey: string): Record<string, unknown>[] {
    if (!Array.isArray(values) || !values.length) fail('expected nonempty array')
    const ids = new Set<string>()
    const orders = new Set<number>()
    for (const value of values) {
      if (!value || typeof value !== 'object') fail('expected definition')
      const id = value[idKey]
      if (typeof id !== 'string' || !identifier.test(id) || id.trim() !== id || ids.has(id))
        fail('invalid or duplicate identifier')
      if (
        !Number.isInteger(value.order) ||
        value.order < 1 ||
        value.order > 2147483647 ||
        orders.has(value.order)
      )
        fail('invalid or duplicate order')
      ids.add(id)
      orders.add(value.order)
    }
    return [...values].sort((a, b) => a.order - b.order)
  }
  const root = object(document, ['groups'])
  const groups = ordered(root.groups, 'id').map((rawGroup) => {
    const group = object(rawGroup, ['id', 'label', 'order', 'sections'])
    if (!text(group.label)) fail('empty group label')
    const keys = new Set<string>()
    const sections = ordered(group.sections, 'id').map((rawSection) => {
      const section = object(rawSection, ['id', 'label', 'order', 'fields'])
      if (!text(section.label)) fail('empty section label')
      const fields = ordered(section.fields, 'key').map((rawField) => {
        const field = object(rawField, [
          'key',
          'default',
          'label',
          'order',
          'input',
          'required',
          'maxLength',
          'format',
        ])
        if (keys.has(field.key as string)) fail('duplicate key across sections')
        keys.add(field.key as string)
        if (
          !text(field.label) ||
          typeof field.default !== 'string' ||
          typeof field.required !== 'boolean' ||
          !Number.isSafeInteger(field.maxLength) ||
          (field.maxLength as number) < 1 ||
          (field.maxLength as number) > 2147483647 ||
          !['text', 'multiline'].includes(field.input as string) ||
          !['plain', 'email', 'phone'].includes(field.format as string)
        )
          fail('invalid field metadata')
        const typed = field as unknown as SiteTextFieldDefinition
        if (
          typed.default.includes('\r') ||
          /<\/?[a-z!][^>]*>|&(?:#\d+|#x[0-9a-f]+|[a-z]+);/iu.test(typed.default) ||
          validateSiteTextValue(typed, typed.default)
        )
          fail(`invalid default: ${group.id}/${typed.key}`)
        return Object.freeze({ ...typed })
      })
      return Object.freeze({ ...section, fields: Object.freeze(fields) })
    })
    return Object.freeze({ ...group, sections: Object.freeze(sections) })
  })
  return Object.freeze({ groups: Object.freeze(groups) }) as unknown as SiteTextRegistryDefinition
}

export const siteTextRegistry = createSiteTextRegistry(document)

/** Exact, case-sensitive lookup; no trimming, lowercasing or identifier normalization. */
export function getSiteTextGroup(group: string): SiteTextGroupDefinition | undefined {
  return siteTextRegistry.groups.find((definition) => definition.id === group)
}

export function getSiteTextFields(
  group: SiteTextGroupDefinition,
): readonly SiteTextFieldDefinition[] {
  return Object.freeze(group.sections.flatMap((section) => section.fields))
}

export function getSiteTextDefaults(
  group: SiteTextGroupDefinition,
): Readonly<Record<string, string>> {
  return Object.freeze(
    Object.fromEntries(getSiteTextFields(group).map((field) => [field.key, field.default])),
  )
}
