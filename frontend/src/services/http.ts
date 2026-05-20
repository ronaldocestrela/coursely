import axios from 'axios'

export const http = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:5230',
  headers: {
    'Content-Type': 'application/json',
  },
})
