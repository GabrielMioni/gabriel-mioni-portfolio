import axios from 'axios'

export const fetchWeatherForecast = async () => {
  return axios.get('http://localhost:5000/WeatherForecast')
}
