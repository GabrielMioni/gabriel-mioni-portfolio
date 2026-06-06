type MenuItemBase = {
  title: string
  icon?: string
  itemClass?: string
  disabled?: boolean
}

export type MenuItem =
  | (MenuItemBase & { route: string; action?: never })
  | (MenuItemBase & { action: () => void | Promise<void>; route?: never })
