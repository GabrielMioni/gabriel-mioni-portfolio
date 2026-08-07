import { describe, expect, it } from 'vitest'
import {
  moveEditorItem,
  normalizeEditorItemsSortOrder,
  removeEditorItem,
  restoreEditorItem
} from '~/utils/editorItems'
import type { BaseEditorItem } from '~/types/editor-items'

const createEditorItem = (
  overrides: Partial<BaseEditorItem> = {}
): BaseEditorItem => ({
  id: 'item-id',
  clientId: 'item-client-id',
  isRemoved: false,
  sort: 0,
  ...overrides
})

describe('normalizeEditorItemsSortOrder', () => {
  it('returns items with zero-based sequential sort values', () => {
    const items = [
      createEditorItem({ clientId: 'first', sort: 20 }),
      createEditorItem({ clientId: 'second', sort: 10 })
    ]

    const normalizedItems = normalizeEditorItemsSortOrder(items)

    expect(normalizedItems.map(item => item.sort)).toEqual([0, 1])
  })

  it('does not mutate the original items', () => {
    const items = [
      createEditorItem({ clientId: 'first', sort: 20 }),
      createEditorItem({ clientId: 'second', sort: 10 })
    ]

    const normalizedItems = normalizeEditorItemsSortOrder(items)

    expect(items.map(item => item.sort)).toEqual([20, 10])
    expect(normalizedItems).not.toBe(items)
    expect(normalizedItems[0]).not.toBe(items[0])
    expect(normalizedItems[1]).not.toBe(items[1])
  })
})

describe('removeEditorItem', () => {
  it('marks a saved item as removed without mutating the original item', () => {
    const items = [
      createEditorItem({
        id: 'first-id',
        clientId: 'first',
        sort: 4
      }),
      createEditorItem({
        id: 'second-id',
        clientId: 'second',
        sort: 8
      })
    ]

    const updatedItems = removeEditorItem('first', items)

    expect(updatedItems).toEqual([
      expect.objectContaining({
        clientId: 'first',
        isRemoved: true,
        sort: 0
      }),
      expect.objectContaining({
        clientId: 'second',
        isRemoved: false,
        sort: 1
      })
    ])
    expect(items[0]!.isRemoved).toBe(false)
    expect(items.map(item => item.sort)).toEqual([4, 8])
  })

  it('removes an unsaved item and normalizes the remaining sort values', () => {
    const items = [
      createEditorItem({
        id: 'first-id',
        clientId: 'first',
        sort: 2
      }),
      createEditorItem({
        id: null,
        clientId: 'unsaved',
        sort: 4
      }),
      createEditorItem({
        id: 'last-id',
        clientId: 'last',
        sort: 6
      })
    ]

    const updatedItems = removeEditorItem('unsaved', items)

    expect(updatedItems.map(item => ({
      clientId: item.clientId,
      sort: item.sort
    }))).toEqual([
      { clientId: 'first', sort: 0 },
      { clientId: 'last', sort: 1 }
    ])
    expect(items).toHaveLength(3)
  })
})

describe('restoreEditorItem', () => {
  it('marks a saved item as active without mutating the original item', () => {
    const items = [
      createEditorItem({
        id: 'first-id',
        clientId: 'first',
        isRemoved: true,
        sort: 4
      }),
      createEditorItem({
        id: 'second-id',
        clientId: 'second',
        sort: 8
      })
    ]

    const updatedItems = restoreEditorItem('first', items)

    expect(updatedItems).toEqual([
      expect.objectContaining({
        clientId: 'first',
        isRemoved: false,
        sort: 0
      }),
      expect.objectContaining({
        clientId: 'second',
        isRemoved: false,
        sort: 1
      })
    ])
    expect(items[0]!.isRemoved).toBe(true)
    expect(items.map(item => item.sort)).toEqual([4, 8])
  })
})

describe('moveEditorItem', () => {
  it('moves an active item up and normalizes sort values', () => {
    const items = [
      createEditorItem({ clientId: 'first', sort: 4 }),
      createEditorItem({ clientId: 'second', sort: 8 }),
      createEditorItem({ clientId: 'third', sort: 12 })
    ]

    const updatedItems = moveEditorItem('second', items, 'up')

    expect(updatedItems.map(item => ({ clientId: item.clientId, sort: item.sort }))).toEqual([
      { clientId: 'second', sort: 0 },
      { clientId: 'first', sort: 1 },
      { clientId: 'third', sort: 2 }
    ])
    expect(items.map(item => item.clientId)).toEqual(['first', 'second', 'third'])
  })

  it('moves around removed items using the visible item order', () => {
    const items = [
      createEditorItem({ clientId: 'first' }),
      createEditorItem({ clientId: 'removed', isRemoved: true }),
      createEditorItem({ clientId: 'third' })
    ]

    const updatedItems = moveEditorItem('third', items, 'up')

    expect(updatedItems.map(item => item.clientId)).toEqual([
      'third',
      'removed',
      'first'
    ])
  })

  it('leaves an item at the edge in place', () => {
    const items = [
      createEditorItem({ clientId: 'first' }),
      createEditorItem({ clientId: 'second' })
    ]

    expect(moveEditorItem('first', items, 'up')).toBe(items)
    expect(moveEditorItem('second', items, 'down')).toBe(items)
  })
})

describe('missing editor item handling', () => {
  it.each([
    {
      operation: 'remove',
      updateItems: (items: BaseEditorItem[]) =>
        removeEditorItem('missing', items)
    },
    {
      operation: 'restore',
      updateItems: (items: BaseEditorItem[]) =>
        restoreEditorItem('missing', items)
    }
  ])('throws when $operation cannot find the client id', ({ updateItems }) => {
    const items = [createEditorItem()]

    expect(() => updateItems(items))
      .toThrowError('Item with clientId missing not found')
  })
})
