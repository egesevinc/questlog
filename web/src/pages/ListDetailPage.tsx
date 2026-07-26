import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import {
  deleteList,
  getList,
  removeListItem,
  reorderListItems,
  updateList,
  type GameListDetail,
  type GameListItem,
} from '../api/lists'
import { useAuth } from '../auth/AuthContext'
import { getErrorMessage } from '../api/errors'

export function ListDetailPage() {
  const { listId } = useParams<{ listId: string }>()
  const { user } = useAuth()
  const navigate = useNavigate()
  const [list, setList] = useState<GameListDetail | null>(null)
  const [items, setItems] = useState<GameListItem[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [actionError, setActionError] = useState<string | null>(null)
  const [dragIndex, setDragIndex] = useState<number | null>(null)

  // Edit (rename) state.
  const [editing, setEditing] = useState(false)
  const [editTitle, setEditTitle] = useState('')
  const [editDescription, setEditDescription] = useState('')
  const [savingEdit, setSavingEdit] = useState(false)

  // Filters.
  const [genreFilter, setGenreFilter] = useState('')
  const [minRating, setMinRating] = useState(0)

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

  const isOwner = user?.userId === list?.userId

  // Genres present across the list, for the filter dropdown.
  const genres = useMemo(
    () => Array.from(new Set(items.flatMap((i) => i.genres))).sort(),
    [items],
  )

  const filtersActive = genreFilter !== '' || minRating > 0
  const visibleItems = useMemo(
    () =>
      items.filter(
        (i) =>
          (genreFilter === '' || i.genres.includes(genreFilter)) &&
          (minRating === 0 || (i.averageRating != null && i.averageRating >= minRating)),
      ),
    [items, genreFilter, minRating],
  )

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

  const startEditing = () => {
    if (!list) return
    setEditTitle(list.title)
    setEditDescription(list.description ?? '')
    setActionError(null)
    setEditing(true)
  }

  const handleSaveEdit = async (e: FormEvent) => {
    e.preventDefault()
    if (!listId || !list || !editTitle.trim()) return
    setSavingEdit(true)
    setActionError(null)
    try {
      const updated = await updateList(listId, {
        title: editTitle.trim(),
        description: editDescription.trim() || null,
        isPublic: list.isPublic,
      })
      setList(updated)
      setItems(updated.items)
      setEditing(false)
    } catch (err) {
      setActionError(getErrorMessage(err, 'Could not save the list.'))
    } finally {
      setSavingEdit(false)
    }
  }

  const handleDeleteList = async () => {
    if (!listId) return
    if (!window.confirm('Delete this list? This cannot be undone.')) return
    setActionError(null)
    try {
      await deleteList(listId)
      navigate('/lists')
    } catch (err) {
      setActionError(getErrorMessage(err, 'Could not delete the list.'))
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

  // Drag only makes sense on the full, unfiltered list.
  const canDrag = isOwner && !filtersActive

  return (
    <div>
      {editing ? (
        <form onSubmit={handleSaveEdit} className="mb-6 flex flex-col gap-3 max-w-xl">
          <input
            value={editTitle}
            onChange={(e) => setEditTitle(e.target.value)}
            placeholder="List title"
            className="bg-surface border border-border rounded px-3 py-2 text-text text-lg focus:outline-none focus:border-accent"
          />
          <textarea
            value={editDescription}
            onChange={(e) => setEditDescription(e.target.value)}
            placeholder="Description (optional)"
            rows={2}
            className="bg-surface border border-border rounded px-3 py-2 text-text focus:outline-none focus:border-accent resize-none"
          />
          <div className="flex gap-2">
            <button
              type="submit"
              disabled={savingEdit || !editTitle.trim()}
              className="bg-accent text-base font-medium rounded px-4 py-2 hover:bg-accent-hover transition-colors disabled:opacity-50 cursor-pointer"
            >
              {savingEdit ? 'Saving…' : 'Save'}
            </button>
            <button
              type="button"
              onClick={() => setEditing(false)}
              className="text-text-muted hover:text-text transition-colors px-4 py-2 cursor-pointer"
            >
              Cancel
            </button>
          </div>
        </form>
      ) : (
        <div className="mb-4">
          <div className="flex items-start justify-between gap-4">
            <h1 className="text-2xl font-semibold text-text mb-1">{list.title}</h1>
            {isOwner && (
              <div className="shrink-0 flex gap-3 pt-1">
                <button
                  onClick={startEditing}
                  className="text-sm text-accent hover:underline cursor-pointer"
                >
                  Edit
                </button>
                <button
                  onClick={handleDeleteList}
                  className="text-sm text-text-muted hover:text-red-400 transition-colors cursor-pointer"
                >
                  Delete
                </button>
              </div>
            )}
          </div>
          {list.description && <p className="text-text-muted">{list.description}</p>}
        </div>
      )}

      {canDrag && items.length > 1 && (
        <p className="text-xs text-text-muted mb-4">Drag covers to reorder.</p>
      )}
      {actionError && <p className="text-sm text-red-400 mb-4">{actionError}</p>}

      {/* Filters — only worth showing once there are a few games. */}
      {items.length > 1 && (
        <div className="flex flex-wrap items-center gap-3 mb-6">
          <select
            value={genreFilter}
            onChange={(e) => setGenreFilter(e.target.value)}
            className="bg-surface border border-border rounded px-3 py-1.5 text-sm text-text focus:outline-none focus:border-accent cursor-pointer"
          >
            <option value="">All genres</option>
            {genres.map((g) => (
              <option key={g} value={g}>
                {g}
              </option>
            ))}
          </select>
          <select
            value={minRating}
            onChange={(e) => setMinRating(Number(e.target.value))}
            className="bg-surface border border-border rounded px-3 py-1.5 text-sm text-text focus:outline-none focus:border-accent cursor-pointer"
          >
            <option value={0}>Any rating</option>
            <option value={6}>6+</option>
            <option value={7}>7+</option>
            <option value={8}>8+</option>
            <option value={9}>9+</option>
          </select>
          {filtersActive && (
            <>
              <span className="text-sm text-text-muted">
                {visibleItems.length} of {items.length}
              </span>
              <button
                onClick={() => {
                  setGenreFilter('')
                  setMinRating(0)
                }}
                className="text-sm text-accent hover:underline cursor-pointer"
              >
                Clear
              </button>
            </>
          )}
        </div>
      )}

      {items.length === 0 ? (
        <p className="text-text-muted">
          No games in this list yet.{' '}
          <Link to="/search" className="text-accent hover:underline">
            Find one to add
          </Link>
          .
        </p>
      ) : visibleItems.length === 0 ? (
        <p className="text-text-muted">No games match these filters.</p>
      ) : (
        <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-5 gap-4">
          {visibleItems.map((item) => {
            const index = items.indexOf(item)
            return (
              <div
                key={item.id}
                className={`group relative ${dragIndex === index ? 'opacity-50' : ''}`}
                draggable={canDrag}
                onDragStart={() => canDrag && setDragIndex(index)}
                onDragOver={(e) => {
                  if (canDrag) {
                    e.preventDefault()
                    onDragOver(index)
                  }
                }}
                onDragEnd={onDrop}
              >
                <Link to={`/games/${item.igdbId}`} draggable={false}>
                  <div
                    className={`aspect-[3/4] bg-surface border border-border rounded overflow-hidden mb-2 group-hover:border-accent transition-colors relative ${
                      canDrag ? 'cursor-move' : ''
                    }`}
                  >
                    {item.coverUrl ? (
                      <img src={item.coverUrl} alt={item.gameName} className="w-full h-full object-cover" draggable={false} />
                    ) : (
                      <div className="w-full h-full flex items-center justify-center text-text-muted text-xs px-2 text-center">
                        {item.gameName}
                      </div>
                    )}
                    {item.averageRating != null && (
                      <div className="absolute bottom-1 right-1 bg-base/90 text-accent text-xs font-semibold rounded px-1.5 py-0.5">
                        {item.averageRating}
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
            )
          })}
        </div>
      )}
    </div>
  )
}
