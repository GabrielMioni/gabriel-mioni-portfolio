import axios from '@/api/axios'

export const fetchWeatherForecast = async () => {
  return axios.get('/WeatherForecast')
}

export const uploadFile = async (file) => {
  const formData = new FormData()
  formData.append('file', file)
  return axios.post('/api/Files', formData)
}

export const uploadProjectFile = async (file, projectId) => {
  const formData = new FormData()
  formData.append('file', file)
  return axios.post(`/api/Files/project/${projectId}`, formData)
}
