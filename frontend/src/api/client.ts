import axios from 'axios';

const apiClient = axios.create({
  baseURL: 'http://api.localhost/', // Replace with your API base URL
  headers: {
    'Content-Type': 'application/json',
  },
})

apiClient.interceptors.request.use(
    (response) => response,
    (error) => {
        console.error('API Error:', error.response?.data || error.message)
        return Promise.reject(error);
    }
);

export default apiClient;