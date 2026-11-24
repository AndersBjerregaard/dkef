import { afterEach, describe, expect, it, vi } from 'vitest'

afterEach(() => {
  vi.unstubAllEnvs()
  vi.resetModules()
})

describe('site text URLs', () => {
  it('encodes the entire group segment for GET and PATCH', async () => {
    vi.stubEnv('VITE_MODE', 'Test')
    const urls = (await import('../urlservice')).default
    expect(urls.getSiteTexts('member-advantages')).toBe('/SiteTexts/member-advantages')
    expect(urls.patchSiteTexts('home/a ?#')).toBe('/SiteTexts/home%2Fa%20%3F%23')
  })

  it('explicitly rejects unavailable Development endpoints rather than faking an empty group', async () => {
    vi.stubEnv('VITE_MODE', 'Development')
    const urls = (await import('../urlservice')).default
    expect(() => urls.getSiteTexts('home')).toThrow('Development')
    expect(() => urls.patchSiteTexts('home')).toThrow('Development')
  })
})
