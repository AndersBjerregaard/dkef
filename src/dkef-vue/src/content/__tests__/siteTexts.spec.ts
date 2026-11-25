import { readFileSync } from 'node:fs'
import { describe, expect, it } from 'vitest'
import document from '../siteTextDefinitions.json'
import {
  createSiteTextRegistry,
  getSiteTextDefaults,
  getSiteTextFields,
  getSiteTextGroup,
  siteTextRegistry,
} from '../siteTexts'
import { normalizeSiteText, validateSiteTextValue } from '../siteTextRules'
import type { SiteTextFieldDefinition } from '@/types/siteTexts'

// Checkout-only access. Neither runtime package imports anything from its sibling package.
const readCheckout = (path: string) => readFileSync(new URL(path, import.meta.url), 'utf8')

describe('site-text definitions', () => {
  it('matches the complete backend registry, including defaults and validation metadata', () => {
    expect(document).toEqual(
      JSON.parse(readCheckout('../../../../dkef-api/Configuration/siteTextDefinitions.json')),
    )
  })

  it('contains the supported text groups and field counts in editor order', () => {
    expect(siteTextRegistry.groups.map((group) => [group.id, getSiteTextFields(group).length])).toEqual([
      ['about', 15],
      ['home', 27],
      ['membership', 5],
      ['member-advantages', 26],
    ])
  })

  it('has immutable, locally available defaults and exact case-sensitive identifiers', () => {
    const home = getSiteTextGroup('home')!
    expect(getSiteTextDefaults(home)['hero.title']).toBe('Dit netværk i elbranchen')
    expect(getSiteTextGroup('Home')).toBeUndefined()
    expect(getSiteTextGroup(' home')).toBeUndefined()
    expect(getSiteTextDefaults(home)['Hero.title']).toBeUndefined()
    expect(Object.isFrozen(siteTextRegistry.groups)).toBe(true)
    expect(Object.isFrozen(home.sections[0].fields[0])).toBe(true)
    expect(Object.isFrozen(getSiteTextDefaults(home))).toBe(true)
    expect(() => Object.assign(home.sections[0].fields[0], { default: 'Ændret' })).toThrow()
  })

  const mutations: [string, (value: typeof document) => void][] = [
    [
      'duplicate group',
      (value) => {
        value.groups.push(value.groups[0])
      },
    ],
    [
      'duplicate section',
      (value) => {
        value.groups[0].sections.push(value.groups[0].sections[0])
      },
    ],
    [
      'duplicate key',
      (value) => {
        value.groups[0].sections[0].fields.push(value.groups[0].sections[0].fields[0])
      },
    ],
    [
      'cross-section key',
      (value) => {
        value.groups[0].sections[1].fields[0].key = 'page.title'
      },
    ],
    [
      'unknown input',
      (value) => {
        value.groups[0].sections[0].fields[0].input = 'html'
      },
    ],
    [
      'unknown format',
      (value) => {
        value.groups[0].sections[0].fields[0].format = 'url'
      },
    ],
    [
      'nonpositive max',
      (value) => {
        value.groups[0].sections[0].fields[0].maxLength = 0
      },
    ],
    [
      'fractional max',
      (value) => {
        value.groups[0].sections[0].fields[0].maxLength = 1.5
      },
    ],
    [
      'duplicate group order',
      (value) => {
        value.groups[1].order = 1
      },
    ],
    [
      'duplicate section order',
      (value) => {
        value.groups[0].sections[1].order = 1
      },
    ],
    [
      'nonpositive field order',
      (value) => {
        value.groups[0].sections[0].fields[0].order = 0
      },
    ],
    [
      'fractional order',
      (value) => {
        value.groups[0].order = 1.5
      },
    ],
    [
      'empty required default',
      (value) => {
        value.groups[0].sections[0].fields[0].default = ' '
      },
    ],
    [
      'oversized default',
      (value) => {
        value.groups[0].sections[0].fields[0].maxLength = 1
      },
    ],
    [
      'newline default',
      (value) => {
        value.groups[0].sections[0].fields[0].default = 'A\nB'
      },
    ],
    [
      'markup default',
      (value) => {
        value.groups[0].sections[0].fields[0].default = '<b>Hej</b>'
      },
    ],
    [
      'invalid format default',
      (value) => {
        value.groups[0].sections[0].fields[0].format = 'email'
      },
    ],
    [
      'empty group',
      (value) => {
        value.groups[0].sections = []
      },
    ],
    [
      'identifier normalization',
      (value) => {
        value.groups[0].id = 'About'
      },
    ],
    [
      'identifier newline',
      (value) => {
        value.groups[0].id = 'about\n'
      },
    ],
    [
      'oversized group identifier',
      (value) => {
        value.groups[0].id = 'a'.repeat(129)
      },
    ],
    [
      'oversized key identifier',
      (value) => {
        value.groups[0].sections[0].fields[0].key = 'a'.repeat(129)
      },
    ],
  ]
  it.each(mutations)('rejects %s', (_, mutate) => {
    const copy = JSON.parse(JSON.stringify(document)) as typeof document
    mutate(copy)
    expect(() => createSiteTextRegistry(copy)).toThrow('Invalid site-text registry:')
  })

  it.each([null, {}, { groups: [] }, { groups: null }, { groups: [null] }])(
    'rejects malformed shape %j',
    (value) => {
      expect(() => createSiteTextRegistry(value)).toThrow('Invalid site-text registry:')
    },
  )

  it('rejects missing and unknown metadata rather than silently defaulting', () => {
    const copy = JSON.parse(JSON.stringify(document))
    delete copy.groups[0].sections[0].fields[0].required
    expect(() => createSiteTextRegistry(copy)).toThrow('missing or unknown member')
    expect(() => createSiteTextRegistry({ ...document, extra: true })).toThrow(
      'missing or unknown member',
    )
  })

  it('uses explicit metadata order rather than incidental JSON order', () => {
    const copy = JSON.parse(JSON.stringify(document)) as typeof document
    copy.groups.reverse()
    copy.groups.forEach((group) => {
      group.sections.reverse()
      group.sections.forEach((section) => section.fields.reverse())
    })
    expect(createSiteTextRegistry(copy)).toEqual(siteTextRegistry)
  })

  it('accepts identifiers at the storage bound', () => {
    const copy = JSON.parse(JSON.stringify(document)) as typeof document
    copy.groups[0].id = 'a'.repeat(128)
    copy.groups[0].sections[0].fields[0].key = 'a'.repeat(128)
    expect(createSiteTextRegistry(copy).groups[0].id).toHaveLength(128)
  })
})

interface ValueCase {
  name: string
  input: SiteTextFieldDefinition['input']
  format: SiteTextFieldDefinition['format']
  required: boolean
  maxLength: number
  value: string
  normalized: string
  valid: boolean
}
const cases: ValueCase[] = JSON.parse(
  readCheckout('../../../../../tests/site-text-value-cases.json'),
)

describe('shared C#/JavaScript value contract', () => {
  it.each(cases)('$name', (test) => {
    const field = { ...test, key: 'test', label: 'Test', order: 1, default: '' }
    expect(normalizeSiteText(test.value)).toBe(test.normalized)
    expect(validateSiteTextValue(field, test.value) === undefined).toBe(test.valid)
  })

  it('validates all approved defaults', () => {
    siteTextRegistry.groups.forEach((group) => {
      getSiteTextFields(group).forEach((field) => {
        expect(validateSiteTextValue(field, field.default)).toBeUndefined()
      })
    })
  })
})
