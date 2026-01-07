import { ref } from 'vue'

export function useNavMenu(navItems) {
  const isExternal = (url) =>
    url.startsWith('http') ||
    url.startsWith('https') ||
    url.startsWith('mailto:') ||
    url.startsWith('tel:')

  const menuItems = ref(
    navItems.map((item) => ({
      ...item,
      external: isExternal(item.to),
    })),
  )

  return {
    menuItems,
    isExternal,
  }
}
