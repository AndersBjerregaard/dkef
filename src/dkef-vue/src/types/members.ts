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
