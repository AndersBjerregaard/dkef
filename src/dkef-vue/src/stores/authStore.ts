import { ref, computed } from 'vue'
import { defineStore } from 'pinia'
import api from '@/services/apiservice'
import { toast } from 'vue-sonner'

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
    const isAuthReady = ref(false)
    const hasShownSessionExpiryToast = ref(false)

    let initializationPromise: Promise<void> | null = null
    let sessionValidationPromise: Promise<boolean> | null = null

    // Computed
    const isAuthenticated = computed(() => !!accessToken.value && !!user.value)
    const isBoardMember = computed(() => {
      return isAuthenticated.value && !!user.value?.roles?.includes('Board Member')
    })

    const isAdmin = computed(() => {
      return isAuthenticated.value && !!user.value?.roles?.includes('Admin')
    })

    // Actions
    async function login(credentials: LoginDto): Promise<void> {
      try {
        const response = await api.post<TokenResponse>('/auth/login', credentials)
        setTokens(response.data.accessToken, response.data.refreshToken)
        // Decode user info from token or fetch user profile
        await fetchUserProfile()
        hasShownSessionExpiryToast.value = false
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
          name: registrationData.name,
        }
        hasShownSessionExpiryToast.value = false
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
      hasShownSessionExpiryToast.value = false
    }

    async function refreshAccessToken(): Promise<string> {
      if (!refreshToken.value) {
        throw new Error('No refresh token available')
      }

      const response = await api.post<TokenResponse>('/auth/refresh', {
        refreshToken: refreshToken.value,
      })
      setTokens(response.data.accessToken, response.data.refreshToken)
      return response.data.accessToken
    }

    async function initializeSession(): Promise<void> {

      if (initializationPromise) {
        return initializationPromise
      }

      initializationPromise = (async () => {
        try {
          if (!accessToken.value && !refreshToken.value) {
            clearAuth()
            return
          }

          await ensureValidSession({ notifyOnExpiry: false })
        } finally {
          isAuthReady.value = true
          initializationPromise = null
        }
      })()

      return initializationPromise
    }

    async function ensureValidSession(options?: { notifyOnExpiry?: boolean }): Promise<boolean> {
      const notifyOnExpiry = options?.notifyOnExpiry ?? true

      if (!accessToken.value) {
        if (refreshToken.value) {
          try {
            await refreshAccessToken()
            await fetchUserProfile()
            hasShownSessionExpiryToast.value = false
            return true
          } catch {
            handleSessionExpired(notifyOnExpiry)
            return false
          }
        }

        return false
      }

      if (!isTokenExpired(accessToken.value)) {
        if (!user.value) {
          await fetchUserProfile()
        }
        return true
      }

      if (!refreshToken.value) {
        handleSessionExpired(notifyOnExpiry)
        return false
      }

      if (sessionValidationPromise) {
        return sessionValidationPromise
      }

      sessionValidationPromise = (async () => {
        try {
          await refreshAccessToken()
          await fetchUserProfile()
          hasShownSessionExpiryToast.value = false
          return true
        } catch {
          handleSessionExpired(notifyOnExpiry)
          return false
        } finally {
          sessionValidationPromise = null
        }
      })()

      return sessionValidationPromise
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
            name:
              (payload.given_name as string) ||
              (payload.family_name as string) ||
              (payload.name as string) ||
              '',
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
      hasShownSessionExpiryToast.value = false
    }

    function clearAuth(): void {
      accessToken.value = null
      refreshToken.value = null
      user.value = null
    }

    function handleSessionExpired(notify = true): void {
      clearAuth()
      if (notify && !hasShownSessionExpiryToast.value) {
        toast.error('Session udløbet', {
          description: 'Din session er udløbet. Log ind igen for at fortsætte.',
          duration: 5000,
        })
        hasShownSessionExpiryToast.value = true
      }
    }

    function isTokenExpired(token: string): boolean {
      const payload = parseJwt(token)
      const exp = payload.exp

      if (typeof exp !== 'number' && typeof exp !== 'string') {
        return true
      }

      const parsedExp = typeof exp === 'string' ? Number.parseInt(exp, 10) : exp
      if (Number.isNaN(parsedExp)) {
        return true
      }

      const now = Math.floor(Date.now() / 1000)
      const skewSeconds = 30
      return parsedExp <= now + skewSeconds
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

    return {
      // State
      accessToken,
      refreshToken,
      user,
      isAuthReady,
      // Computed
      isAuthenticated,
      isBoardMember,
      isAdmin,
      // Actions
      login,
      register,
      logout,
      refreshAccessToken,
      forgotPassword,
      resetPassword,
      changePassword,
      fetchUserProfile,
      initializeSession,
      ensureValidSession,
      handleSessionExpired,
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
