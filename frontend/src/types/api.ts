export interface PhotoUrlDto {
  originalUrl: string
  largeUrl: string
  previewUrl: string
}

export interface Album {
  albumId: string
  name: string
  photosUrls: PhotoUrlDto[]
}
