import { ref, computed } from 'vue'
import { defineStore } from 'pinia'
import api from '@/services/apiservice'
import type {
  LoginDto,
  RegisterDto,
  TokenResponse,
  ForgotPasswordDto,
  ResetPasswordDto,
  ChangePasswordDto,
  User,
} from '@/types/auth'

export const useAuthStore = defineStore(
  'auth',
  () => {
    // State
    const accessToken = ref<string | null>(null)
    const refreshToken = ref<string | null>(null)
    const user = ref<User | null>(null)

    // Computed
    const isAuthenticated = computed(() => !!accessToken.value && !!user.value)
    const isAdmin = computed(() => {
      return isAuthenticated.value && user.value?.roles?.includes('Admin')
    })

    // Actions
    async function login(credentials: LoginDto): Promise<void> {
      try {
        const response = await api.post<TokenResponse>('/auth/login', credentials)
        setTokens(response.data.accessToken, response.data.refreshToken)
        // Decode user info from token or fetch user profile
        await fetchUserProfile()
      } catch (error) {
        clearAuth()
        throw error
      }
    }

    async function register(registrationData: RegisterDto): Promise<void> {
      try {
        const response = await api.post<TokenResponse>('/auth/register', registrationData)
        setTokens(response.data.accessToken, response.data.refreshToken)
        // Set user info from registration data
        user.value = {
          email: registrationData.email,
          firstName: registrationData.firstName,
          lastName: registrationData.lastName,
        }
      } catch (error) {
        clearAuth()
        throw error
      }
    }

    async function logout(): Promise<void> {
      // Revoke refresh token on the backend if available
      if (refreshToken.value) {
        try {
          await api.post('/auth/logout', { refreshToken: refreshToken.value })
        } catch (error) {
          console.error('Failed to revoke refresh token on server:', error)
          // Continue with local logout even if backend call fails
        }
      }
      clearAuth()
    }

    async function refreshAccessToken(): Promise<string> {
      if (!refreshToken.value) {
        throw new Error('No refresh token available')
      }

      try {
        const response = await api.post<TokenResponse>('/auth/refresh', {
          refreshToken: refreshToken.value,
        })
        setTokens(response.data.accessToken, response.data.refreshToken)
        return response.data.accessToken
      } catch (error) {
        clearAuth()
        throw error
      }
    }

    async function forgotPassword(email: string): Promise<void> {
      const dto: ForgotPasswordDto = { email }
      await api.post('/auth/forgot', dto)
    }

    async function resetPassword(resetData: ResetPasswordDto): Promise<void> {
      await api.post('/auth/reset', resetData)
    }

    async function changePassword(changeData: ChangePasswordDto): Promise<void> {
      await api.post('/auth/change', changeData)
    }

    async function fetchUserProfile(): Promise<void> {
      // If you have a user profile endpoint, fetch it here
      // For now, we'll decode basic info from the JWT token
      if (accessToken.value) {
        try {
          const payload = parseJwt(accessToken.value)
          // Extract roles - JWT claims can use different formats
          let roles: string[] = []
          if (payload.role) {
            // Single role or array of roles
            roles = Array.isArray(payload.role)
              ? (payload.role as string[])
              : [payload.role as string]
          } else if (payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']) {
            // ASP.NET Core default role claim
            const roleClaim =
              payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
            roles = Array.isArray(roleClaim) ? (roleClaim as string[]) : [roleClaim as string]
          }

          user.value = {
            email: (payload.email as string) || (payload.sub as string) || '',
            firstName: (payload.given_name as string) || (payload.firstName as string) || '',
            lastName: (payload.family_name as string) || (payload.lastName as string) || '',
            roles,
          }
        } catch (error) {
          console.error('Failed to parse JWT token:', error)
        }
      }
    }

    function setTokens(access: string, refresh: string): void {
      accessToken.value = access
      refreshToken.value = refresh
    }

    function clearAuth(): void {
      accessToken.value = null
      refreshToken.value = null
      user.value = null
    }

    function parseJwt(token: string): Record<string, unknown> {
      try {
        const base64Url = token.split('.')[1]
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/')
        const jsonPayload = decodeURIComponent(
          atob(base64)
            .split('')
            .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
            .join(''),
        )
        return JSON.parse(jsonPayload) as Record<string, unknown>
      } catch (error) {
        console.error('Failed to parse JWT:', error)
        return {}
      }
    }

    // Initialize auth state on store creation
    if (accessToken.value && !user.value) {
      fetchUserProfile()
    }

    return {
      // State
      accessToken,
      refreshToken,
      user,
      // Computed
      isAuthenticated,
      isAdmin,
      // Actions
      login,
      register,
      logout,
      refreshAccessToken,
      forgotPassword,
      resetPassword,
      changePassword,
      setTokens,
      clearAuth,
    }
  },
  {
    persist: {
      key: 'auth',
      storage: localStorage,
      pick: ['accessToken', 'refreshToken', 'user'],
    },
  },
)
