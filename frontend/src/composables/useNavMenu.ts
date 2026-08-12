import { ref } from 'vue'

export interface NavItem {
  to: string;
  [key: string]: any;
}

export interface ProcessedNavItem extends NavItem {
  external: boolean;
}

export function useNavMenu(navItems: NavItem[]) {
  const isExternal = (url: string): boolean =>
    url.startsWith('http') ||
    url.startsWith('https') ||
    url.startsWith('mailto:') ||
    url.startsWith('tel:')

  const menuItems = ref<ProcessedNavItem[]>(
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
