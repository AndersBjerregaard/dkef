// Auth-related type definitions matching backend DTOs

export interface LoginDto {
  email: string
  password: string
}

export interface RegisterDto {
  email: string
  password: string
  firstName: string
  lastName: string
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
  email: string
  token: string
  newPassword: string
}

export interface ChangePasswordDto {
  email: string
  currentPassword: string
  newPassword: string
}

export interface User {
  email: string
  firstName: string
  lastName: string
}

export interface AuthState {
  accessToken: string | null
  refreshToken: string | null
  user: User | null
  isAuthenticated: boolean
}
