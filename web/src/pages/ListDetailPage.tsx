import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { getList, removeListItem, type GameListDetail } from '../api/lists'
import { useAuth } from '../auth/AuthContext'
import { getErrorMessage } from '../api/errors'

export function ListDetailPage() {
  const { listId } = useParams<{ listId: string }>()
  const { user } = useAuth()
  const [list, setList] = useState<GameListDetail | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [removeError, setRemoveError] = useState<string | null>(null)

  const load = () => {
    if (!listId) return
    setLoading(true)
    getList(listId)
      .then(setList)
      .catch(() => setError('Could not load this list.'))
      .finally(() => setLoading(false))
  }

  useEffect(load, [listId])

  const handleRemove = async (itemId: string) => {
    if (!listId) return
    setRemoveError(null)
    try {
      await removeListItem(listId, itemId)
      load()
    } catch (err) {
      setRemoveError(getErrorMessage(err, 'Could not remove that game.'))
    }
  }

  if (loading) return <p className="text-text-muted">Loading…</p>
  if (error || !list) return <p className="text-red-400">{error ?? 'List not found.'}</p>

  const isOwner = user?.userId === list.userId

  return (
    <div>
      <h1 className="text-2xl font-semibold text-text mb-1">{list.title}</h1>
      {list.description && <p className="text-text-muted mb-6">{list.description}</p>}
      {removeError && <p className="text-sm text-red-400 mb-4">{removeError}</p>}

      {list.items.length === 0 ? (
        <p className="text-text-muted">
          No games in this list yet.{' '}
          <Link to="/search" className="text-accent hover:underline">
            Find one to add
          </Link>
          .
        </p>
      ) : (
        <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-5 gap-4">
          {list.items.map((item) => (
            <div key={item.id} className="group relative">
              <Link to={`/games/${item.igdbId}`}>
                <div className="aspect-[3/4] bg-surface border border-border rounded overflow-hidden mb-2 group-hover:border-accent transition-colors">
                  {item.coverUrl ? (
                    <img src={item.coverUrl} alt={item.gameName} className="w-full h-full object-cover" />
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
