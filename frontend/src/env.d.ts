/// <reference types="vite/client" />

declare module '*.vue' {
  import type { DefineComponent } from 'vue'
  // Используем object вместо {} и Record<string, unknown> вместо any
  const component: DefineComponent<object, object, Record<string, unknown>>
  export default component
}

// Обычный CSS (import './style.css')
declare module '*.css'

// CSS-модули (import styles from './style.module.css')
declare module '*.module.css' {
  const classes: Record<string, string>
  export default classes
}
