import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import {
  getList,
  removeListItem,
  reorderListItems,
  type GameListDetail,
  type GameListItem,
} from '../api/lists'
import { useAuth } from '../auth/AuthContext'
import { getErrorMessage } from '../api/errors'

export function ListDetailPage() {
  const { listId } = useParams<{ listId: string }>()
  const { user } = useAuth()
  const [list, setList] = useState<GameListDetail | null>(null)
  const [items, setItems] = useState<GameListItem[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [actionError, setActionError] = useState<string | null>(null)
  const [dragIndex, setDragIndex] = useState<number | null>(null)

  const load = () => {
    if (!listId) return
    setLoading(true)
    getList(listId)
      .then((l) => {
        setList(l)
        setItems(l.items)
      })
      .catch(() => setError('Could not load this list.'))
      .finally(() => setLoading(false))
  }

  useEffect(load, [listId])

  const handleRemove = async (itemId: string) => {
    if (!listId) return
    setActionError(null)
    try {
      await removeListItem(listId, itemId)
      setItems((prev) => prev.filter((i) => i.id !== itemId))
    } catch (err) {
      setActionError(getErrorMessage(err, 'Could not remove that game.'))
    }
  }

  const onDragOver = (index: number) => {
    if (dragIndex === null || dragIndex === index) return
    setItems((prev) => {
      const next = [...prev]
      const [moved] = next.splice(dragIndex, 1)
      next.splice(index, 0, moved)
      return next
    })
    setDragIndex(index)
  }

  const onDrop = async () => {
    setDragIndex(null)
    if (!listId) return
    setActionError(null)
    try {
      await reorderListItems(listId, items.map((i) => i.id))
    } catch (err) {
      setActionError(getErrorMessage(err, 'Could not save the new order.'))
      load() // revert to server order
    }
  }

  if (loading) return <p className="text-text-muted">Loading…</p>
  if (error || !list) return <p className="text-red-400">{error ?? 'List not found.'}</p>

  const isOwner = user?.userId === list.userId

  return (
    <div>
      <h1 className="text-2xl font-semibold text-text mb-1">{list.title}</h1>
      {list.description && <p className="text-text-muted mb-2">{list.description}</p>}
      {isOwner && items.length > 1 && (
        <p className="text-xs text-text-muted mb-4">Drag covers to reorder.</p>
      )}
      {actionError && <p className="text-sm text-red-400 mb-4">{actionError}</p>}

      {items.length === 0 ? (
        <p className="text-text-muted">
          No games in this list yet.{' '}
          <Link to="/search" className="text-accent hover:underline">
            Find one to add
          </Link>
          .
        </p>
      ) : (
        <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-5 gap-4">
          {items.map((item, index) => (
            <div
              key={item.id}
              className={`group relative ${dragIndex === index ? 'opacity-50' : ''}`}
              draggable={isOwner}
              onDragStart={() => setDragIndex(index)}
              onDragOver={(e) => {
                if (isOwner) {
                  e.preventDefault()
                  onDragOver(index)
                }
              }}
              onDragEnd={onDrop}
            >
              <Link to={`/games/${item.igdbId}`} draggable={false}>
                <div
                  className={`aspect-[3/4] bg-surface border border-border rounded overflow-hidden mb-2 group-hover:border-accent transition-colors ${
                    isOwner ? 'cursor-move' : ''
                  }`}
                >
                  {item.coverUrl ? (
                    <img src={item.coverUrl} alt={item.gameName} className="w-full h-full object-cover" draggable={false} />
                  ) : (
                    <div className="w-full h-full flex items-center justify-center text-text-muted text-xs px-2 text-center">
                      {item.gameName}
                    </div>
                  )}
                </div>
                <p className="text-sm text-text truncate group-hover:text-accent transition-colors">
                  {item.gameName}
                </p>
              </Link>
              {isOwner && (
                <button
                  onClick={() => handleRemove(item.id)}
                  className="absolute top-1 right-1 bg-base/80 text-text-muted hover:text-red-400 text-xs rounded px-1.5 py-0.5 cursor-pointer"
                >
                  Remove
                </button>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
