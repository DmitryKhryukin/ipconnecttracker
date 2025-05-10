import axios from 'axios'

const api = axios.create({
  baseURL: '/api', // Vite proxy handles this in dev
  timeout: 5000,
})

export default api