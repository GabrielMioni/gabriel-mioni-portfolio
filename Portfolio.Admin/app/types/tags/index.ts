import type { HasId } from '~/types/editor-items'

export type TagEditorItem = HasId & {
  name: string
  value: string | null
}
