export type SiteTextInput = 'text' | 'multiline'
export type SiteTextFormat = 'plain' | 'email' | 'phone'

export interface SiteTextFieldDefinition {
  readonly key: string
  readonly default: string
  readonly label: string
  readonly order: number
  readonly input: SiteTextInput
  readonly required: boolean
  readonly maxLength: number
  readonly format: SiteTextFormat
}

export interface SiteTextSectionDefinition {
  readonly id: string
  readonly label: string
  readonly order: number
  readonly fields: readonly SiteTextFieldDefinition[]
}

export interface SiteTextGroupDefinition {
  readonly id: string
  readonly label: string
  readonly order: number
  readonly sections: readonly SiteTextSectionDefinition[]
}

export interface SiteTextRegistryDefinition {
  readonly groups: readonly SiteTextGroupDefinition[]
}

/** GET and successful PATCH contain overrides only, never merged defaults or audit data. */
export interface SiteTextGroupResponse {
  readonly group: string
  readonly overrides: Readonly<Record<string, string>>
}

/** Only changed keys; no reset/delete/null operation. Validate unknown wire data on the API. */
export interface SiteTextPatchRequest {
  readonly changes: Readonly<Record<string, string>>
}

export interface SiteTextValidationProblem {
  readonly status: 400
  readonly title: string
  readonly type?: string
  readonly errors: Readonly<Record<'request' | `changes.${string}`, readonly string[]>>
}

/** HTTP outcomes; authorization and transactional behavior belong to the API implementation. */
export type SiteTextErrorStatus = 400 | 401 | 403 | 404 | 413 | 500
