import axios from '@/api/axios'

export const fetchWeatherForecast = async () => {
  return axios.get('/WeatherForecast')
}
