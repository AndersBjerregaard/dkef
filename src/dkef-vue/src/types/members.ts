export const enum Section {
  CopenhagenZealand = 0,
  Jutland = 1,
  NorthJutland = 2,
  SouthJutland = 3,
  Funen = 4,
  MainAssociation = 5,
}

export const enum MemberType {
  Member = 0,
  BoardMember = 1,
  Admin = 2,
}

export const SECTION_DISPLAY_MAP: Record<Section, string> = {
  [Section.CopenhagenZealand]: 'København Sjælland Sektion',
  [Section.Jutland]: 'Jydsk Sektion',
  [Section.NorthJutland]: 'Nordjysk Sektion',
  [Section.SouthJutland]: 'Sydjysk Sektion',
  [Section.Funen]: 'Fynsk Sektion',
  [Section.MainAssociation]: 'Hovedforeningen',
}

export const MEMBER_TYPE_DISPLAY_MAP: Record<MemberType, string> = {
  [MemberType.Member]: 'Member',
  [MemberType.BoardMember]: 'Board Member',
  [MemberType.Admin]: 'Admin',
}

export interface Contact {
  id: string
  email: string
  name: string
  address: string
  zip: string
  city: string
  countryCode: string
  cvrNumber: string
  eanNumber: string
  privatePhoneNumber: string
  attPerson: string
  paymentMethod: string
  paymentDeadlineInDays: number
  totalSale: string
  totalPurchase: string
  enrollmentDate: string | null
  subscription: string
  invoiceName2: string
  companyName: string
  companyAddress: string
  companyZIP: string
  companyCity: string
  companyPhone: string
  employmentStatus: string
  primarySection: Section | null
  secondarySection: Section | null
  magazineDelivery: string
  title: string
  memberType: MemberType
}

export interface ContactDto {
  email: string
  name: string
  title: string
  employmentStatus: string
  address: string
  zip: string
  city: string
  companyName: string
  companyAddress: string
  companyZIP: string
  companyCity: string
  cvrNumber: string
  companyPhone: string
  magazineDelivery: string
  eanNumber: string
  primarySection: Section
  secondarySection: Section | null
}

export interface UpdateProfileDto {
  name: string
  title: string
  employmentStatus: string
  address: string
  zip: string
  city: string
  companyName: string
  companyAddress: string
  companyZIP: string
  companyCity: string
  cvrNumber: string
  companyPhone: string
  magazineDelivery: string
  eanNumber: string
  primarySection: Section
  secondarySection: Section | null
}

export interface ContactCollection {
  collection: Contact[]
  total: number
}

export enum Sort {
  None = 'none',
  Asc = 'asc',
  Desc = 'desc',
}

export interface ColumnSortState {
  [key: string]: Sort
}
