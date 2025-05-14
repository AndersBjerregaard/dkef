import axios from 'axios'

const apiUrl = import.meta.env.VITE_API_BASE_URL

console.info('Api URL loaded as: ', apiUrl)

const axiosInstance = axios.create({
  baseURL: apiUrl,
  timeout: 1000,
  // headers: {'X-Custom': '1234'}
})

function get(url: string) {
  return axiosInstance.get(url)
}

export default {
  get,
}
