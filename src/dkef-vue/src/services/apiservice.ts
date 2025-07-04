import axios, { type AxiosRequestConfig, type AxiosResponse } from 'axios'

const apiUrl = import.meta.env.VITE_API_BASE_URL

console.info('Api URL loaded as: ', apiUrl)

const axiosInstance = axios.create({
  baseURL: apiUrl,
  timeout: 10000,
  // headers: {'X-Custom': '1234'}
})

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function get<T = any>(url: string, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> {
  return axiosInstance.get(url, config)
}

export default {
  get,
}
