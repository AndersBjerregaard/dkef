import type { SiteTextFieldDefinition } from '@/types/siteTexts'

export const normalizeSiteText = (value: string): string => value.replace(/\r\n?/g, '\n')

// Explicit union of .NET/ECMAScript whitespace so required/email rules agree across runtimes.
const whitespace = /[\t-\r \u0085\u00a0\u1680\u2000-\u200a\u2028\u2029\u202f\u205f\u3000\ufeff]/u
const nonWhitespace =
  /[^\t-\r \u0085\u00a0\u1680\u2000-\u200a\u2028\u2029\u202f\u205f\u3000\ufeff]/u

export const hasSiteTextContent = (value: string): boolean => nonWhitespace.test(value)

/** Client usability check only. The API also strips markup and revalidates the resulting text. */
export function validateSiteTextValue(
  field: SiteTextFieldDefinition,
  value: string,
): string | undefined {
  // Check raw length as well: CRLF normalization must not bypass the submitted-value limit.
  if (value.length > field.maxLength) return `Teksten må højst være ${field.maxLength} tegn.`
  value = normalizeSiteText(value)
  if (field.required && !hasSiteTextContent(value)) return 'Feltet skal udfyldes.'
  if (field.input === 'text' && /[\n\u0085\u2028\u2029]/u.test(value))
    return 'Feltet må ikke indeholde linjeskift.'
  if (value === '' && !field.required) return
  // EmailAddressAttribute's single-@ convention, tightened to one bare mailbox (no whitespace/display name).
  if (
    field.format === 'email' &&
    (whitespace.test(value) || !/^[^@<>(),;:"]+@[^@<>(),;:"]+$/u.test(value))
  )
    return 'Indtast en gyldig e-mailadresse uden navn eller mellemrum.'
  if (field.format === 'phone' && (!/^[0-9 +()\-]+$/u.test(value) || !/[0-9]/u.test(value)))
    return 'Telefonnummeret må kun indeholde cifre, mellemrum, +, -, ( og ).'
}
