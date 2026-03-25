// Auth-related type definitions matching backend DTOs

export interface LoginDto {
  email: string
  password: string
}

export interface TokenResponse {
  message: string
  accessToken: string
  refreshToken: string
  expiresIn: number
}

export interface RefreshTokenDto {
  refreshToken: string
}

export interface ForgotPasswordDto {
  email: string
}

export interface ResetPasswordDto {
  token: string
  newPassword: string
}

export interface ChangePasswordDto {
  currentPassword: string
  newPassword: string
}

export interface User {
  email: string
  name: string
  roles?: string[]
}

export const enum Section {
  CopenhagenZealand = 0,
  Jutland = 1,
  NorthJutland = 2,
  SouthJutland = 3,
  Funen = 4,
  MainAssociation = 5,
}

export interface RegisterDto {
  email: string
  password: string
  confirmPassword: string
  name: string
  primarySection: Section
  secondarySection: Section | null
  title: string
  address: string
  zip: string
  city: string
  phone: string
  employmentStatus: string
  magazineDelivery: string
  subscription: string
  companyName: string
  companyAddress: string
  companyZIP: string
  companyCity: string
  companyPhone: string
  cvrNumber: string
  eanNumber: string
}

export interface AuthState {
  accessToken: string | null
  refreshToken: string | null
  user: User | null
  isAuthenticated: boolean
}
