// Auth-related type definitions matching backend DTOs

export interface LoginDto {
  email: string
  password: string
}

export interface RegisterDto {
  email: string
  password: string
  confirmPassword: string
  name: string
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

export interface AuthState {
  accessToken: string | null
  refreshToken: string | null
  user: User | null
  isAuthenticated: boolean
}
