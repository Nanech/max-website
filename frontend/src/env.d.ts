// Объясняем TypeScript, как работать с Vue-компонентами
declare module '*.vue' {
  import type { DefineComponent } from 'vue'
  const component: DefineComponent<{}, {}, any>
  export default component
}

// Объясняем TypeScript, что импортировать CSS-файлы — это нормально (Vite сам их применит)
declare module '*.css' {
  const content: Record<string, string>
  export default content
}
