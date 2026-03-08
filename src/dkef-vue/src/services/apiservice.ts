import axios, {
  type AxiosRequestConfig,
  type AxiosResponse,
  type InternalAxiosRequestConfig,
  AxiosError,
} from 'axios'

export interface ApiRequestConfig extends AxiosRequestConfig {
  skipAuth?: boolean
}

const apiUrl = import.meta.env.VITE_API_BASE_URL

console.info('Api URL loaded as: ', apiUrl)

const axiosInstance = axios.create({
  baseURL: apiUrl,
  timeout: 10000,
  // headers: {'X-Custom': '1234'}
})

// Flag to prevent multiple refresh attempts
let isRefreshing = false
let failedQueue: Array<{
  resolve: (value: string) => void
  reject: (error: Error) => void
}> = []

const processQueue = (error: Error | null, token: string | null = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error)
    } else {
      prom.resolve(token!)
    }
  })
  failedQueue = []
}

// Request interceptor to attach access token
axiosInstance.interceptors.request.use(
  (config: InternalAxiosRequestConfig & { skipAuth?: boolean }) => {
    // Skip auth header for public endpoints
    if (config.skipAuth) return config

    // Import dynamically to avoid circular dependency
    const authStore = getAuthStore()
    const token = authStore?.accessToken

    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`
    }

    return config
  },
  (error) => {
    return Promise.reject(error)
  },
)

// Response interceptor to handle token refresh
axiosInstance.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean }

    // If error is 401 and we haven't retried yet
    if (error.response?.status === 401 && !originalRequest._retry) {
      // Skip refresh attempts for auth endpoints to prevent infinite loops
      if (
        originalRequest.url?.includes('/auth/login') ||
        originalRequest.url?.includes('/auth/refresh') ||
        originalRequest.url?.includes('/auth/register')
      ) {
        return Promise.reject(error)
      }

      if (isRefreshing) {
        // If already refreshing, queue this request
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject })
        })
          .then((token) => {
            if (originalRequest.headers) {
              originalRequest.headers.Authorization = `Bearer ${token}`
            }
            return axiosInstance(originalRequest)
          })
          .catch((err) => {
            return Promise.reject(err)
          })
      }

      originalRequest._retry = true
      isRefreshing = true

      const authStore = getAuthStore()

      try {
        const newAccessToken = await authStore?.refreshAccessToken()
        processQueue(null, newAccessToken)

        if (originalRequest.headers && newAccessToken) {
          originalRequest.headers.Authorization = `Bearer ${newAccessToken}`
        }

        return axiosInstance(originalRequest)
      } catch (refreshError) {
        processQueue(
          refreshError instanceof Error ? refreshError : new Error('Token refresh failed'),
          null,
        )
        // Refresh failed, clear auth and redirect to login
        authStore?.clearAuth()
        // Optionally redirect to login page
        if (typeof window !== 'undefined') {
          window.location.href = '/login'
        }
        return Promise.reject(refreshError)
      } finally {
        isRefreshing = false
      }
    }

    return Promise.reject(error)
  },
)

// Helper function to get auth store (avoids circular dependency)
let authStoreInstance: ReturnType<typeof import('@/stores/authStore').useAuthStore> | null = null

function getAuthStore() {
  // Lazy load to avoid circular dependency
  if (!authStoreInstance) {
    try {
      // Dynamic import using import() instead of require()
      import('@/stores/authStore').then(({ useAuthStore }) => {
        authStoreInstance = useAuthStore()
      })
    } catch (error) {
      console.error('Failed to get auth store:', error)
    }
  }
  return authStoreInstance
}

function get<T = unknown>(url: string, config?: ApiRequestConfig): Promise<AxiosResponse<T>> {
  return axiosInstance.get(url, config)
}

function post<T = unknown>(
  url: string,
  data: unknown,
  config?: ApiRequestConfig,
): Promise<AxiosResponse<T>> {
  return axiosInstance.post(url, data, config)
}

function put<T = unknown>(
  url: string,
  data: unknown,
  config?: ApiRequestConfig,
): Promise<AxiosResponse<T>> {
  return axiosInstance.put(url, data, config)
}

function del<T = unknown>(url: string, config?: ApiRequestConfig): Promise<AxiosResponse<T>> {
  return axiosInstance.delete(url, config)
}

export default {
  get,
  post,
  put,
  delete: del,
  axiosInstance, // Export instance for direct access if needed
}
