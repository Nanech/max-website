import { isAxiosError } from 'axios'
import { ref } from 'vue'
import apiClient from '@/api/client'
import { Album } from '@/types/api'

export function useAlbum(albumId: string) {
  const data = ref<Album | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  const fetchAlbum = async () => {
    loading.value = true
    error.value = null

    try {
      const response = await apiClient.get<Album>(`album/${albumId}`)
      data.value = response.data
    } catch (err) {
      if (isAxiosError(err)) {
        error.value = err.response?.data?.message || 'Failed to fetch album'
      } else if (err instanceof Error) {
        error.value = err.message || 'Failed to fetch album'
      } else {
        error.value = 'An unknown error occurred while fetching the album'
      }
    } finally {
      loading.value = false
    }
  }

  return { data, loading, error, fetchAlbum }
}
