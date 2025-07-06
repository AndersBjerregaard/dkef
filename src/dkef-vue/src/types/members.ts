export interface Contact {
  attInvoice: string
  companyAddress: string
  companyCity: string
  companyEmail: string
  companyName: string
  companyPhone: string
  companyZIP: string
  createdAt: string
  cvrNumber: string
  eanNumber: string
  elTeknikDelivery: string
  email: string
  expectedEndDateOfBeingStudent: string
  firstName: string
  helpToStudents: string
  id: string
  invoice: string
  invoiceEmail: string
  lastName: string
  mentor: string
  occupation: string
  oldMemberNumber: string
  primarySection: string
  privateAddress: string
  privateCity: string
  privatePhone: string
  privateZIP: string
  registrationDate: string
  secondarySection: string
  source: string
  title: string
  workTasks: string
}

export interface ContactDto {
  email: string
  firstName: string
  lastName: string
  title: string
  occupation: string
  workTasks: string
  privateAddress: string
  privateZIP: string
  privateCity: string
  privatePhone: string
  companyName: string
  companyAddress: string
  companyZIP: string
  companyCity: string
  cvrNumber: string
  companyPhone: string
  companyEmail: string
  elTeknikDelivery: string
  eanNumber: string
  invoice: string
  helpToStudents: string
  mentor: string
  primarySection: string
  secondarySection: string
  invoiceEmail: string
  oldMemberNumber: string
  attInvoice: string
  expectedEndDateOfBeingStudent: string
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
