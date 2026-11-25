import { readFileSync } from 'node:fs'
import { describe, expect, it } from 'vitest'
import { siteImageGroups, validateSiteImageFile, maxSiteImageBytes } from '../siteImages'

const readCheckout = (path: string) => readFileSync(new URL(path, import.meta.url), 'utf8')

describe('site image inventory', () => {
  it('matches the API allowlist exactly and keeps all ten bundled defaults', () => {
    const apiSlots = JSON.parse(
      readCheckout('../../../../dkef-api/Configuration/siteImageSlots.json'),
    )
    expect(
      Object.fromEntries(
        siteImageGroups.map((group) => [group.id, group.slots.map((slot) => slot.id)]),
      ),
    ).toEqual(apiSlots)
    expect(siteImageGroups.flatMap((group) => group.slots)).toHaveLength(10)
    for (const group of siteImageGroups) {
      for (const slot of group.slots) {
        expect(slot.url).toBeTruthy()
        expect(slot.alt.trim()).not.toBe('')
      }
    }
  })

  it('rejects empty/oversized/non-raster uploads locally and accepts the size boundary', () => {
    expect(validateSiteImageFile(new File([], 'empty.jpg', { type: 'image/jpeg' }))).toContain(
      'tom',
    )
    expect(
      validateSiteImageFile(new File(['svg'], 'bad.svg', { type: 'image/svg+xml' })),
    ).toContain('JPEG')
    expect(
      validateSiteImageFile(
        new File([new Uint8Array(maxSiteImageBytes)], 'valid.webp', { type: 'image/webp' }),
      ),
    ).toBeUndefined()
    expect(
      validateSiteImageFile(
        new File([new Uint8Array(maxSiteImageBytes + 1)], 'big.png', { type: 'image/png' }),
      ),
    ).toContain('5 MB')
  })
})
