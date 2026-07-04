import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import { getGame, type GameDetail } from '../api/games'
import { getMyLists, addListItem, type GameListSummary } from '../api/lists'
import { useAuth } from '../auth/AuthContext'
import { LogGameForm } from '../components/LogGameForm'
import { getErrorMessage } from '../api/errors'

export function GameDetailPage() {
  const { igdbId } = useParams<{ igdbId: string }>()
  const { user } = useAuth()
  const [game, setGame] = useState<GameDetail | null>(null)
  const [loading, setLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)
  const [showLogForm, setShowLogForm] = useState(false)
  const [saved, setSaved] = useState(false)
  const [lists, setLists] = useState<GameListSummary[]>([])
  const [addedToList, setAddedToList] = useState<string | null>(null)
  const [listError, setListError] = useState<string | null>(null)

  useEffect(() => {
    if (!igdbId) return
    setLoading(true)
    getGame(Number(igdbId))
      .then(setGame)
      .catch(() => setLoadError('Could not load this game.'))
      .finally(() => setLoading(false))
  }, [igdbId])

  useEffect(() => {
    if (user) getMyLists().then(setLists).catch(() => {})
  }, [user])

  const handleAddToList = async (listId: string) => {
    if (!game) return
    setListError(null)
    try {
      await addListItem(listId, game.igdbId)
      setAddedToList(listId)
    } catch (err) {
      setListError(getErrorMessage(err, 'Could not add to that list.'))
    }
  }

  if (loading) return <p className="text-text-muted">Loading…</p>
  if (loadError || !game) return <p className="text-red-400">{loadError ?? 'Game not found.'}</p>

  return (
    <div className="grid grid-cols-1 sm:grid-cols-[220px_1fr] gap-8">
      <div className="aspect-[3/4] bg-surface border border-border rounded overflow-hidden">
        {game.coverUrl ? (
          <img src={game.coverUrl} alt={game.name} className="w-full h-full object-cover" />
        ) : null}
      </div>
      <div>
        <h1 className="text-2xl font-semibold text-text mb-1">{game.name}</h1>
        {game.releaseDate && (
          <p className="text-text-muted text-sm mb-4">
            {new Date(game.releaseDate).getFullYear()}
          </p>
        )}
        {game.genres.length > 0 && (
          <p className="text-sm text-text-muted mb-4">{game.genres.join(' · ')}</p>
        )}
        {game.summary && <p className="text-text mb-6 leading-relaxed">{game.summary}</p>}

        {user && !saved && !showLogForm && (
          <button
            onClick={() => setShowLogForm(true)}
            className="bg-accent text-base font-medium rounded px-4 py-2 hover:bg-accent-hover transition-colors cursor-pointer"
          >
            Log this game
          </button>
        )}
        {saved && <p className="text-accent text-sm">Logged!</p>}
        {showLogForm && (
          <LogGameForm
            igdbId={game.igdbId}
            onSaved={() => {
              setShowLogForm(false)
              setSaved(true)
            }}
            onCancel={() => setShowLogForm(false)}
          />
        )}

        {user && lists.length > 0 && (
          <div className="mt-6">
            <p className="text-sm text-text-muted mb-2">Add to a list</p>
            <div className="flex flex-wrap gap-2">
              {lists.map((list) => (
                <button
                  key={list.id}
                  onClick={() => handleAddToList(list.id)}
                  disabled={addedToList === list.id}
                  className="text-sm bg-surface border border-border rounded px-3 py-1.5 hover:border-accent transition-colors disabled:opacity-50 cursor-pointer"
                >
                  {addedToList === list.id ? 'Added ✓' : list.title}
                </button>
              ))}
            </div>
            {listError && <p className="text-sm text-red-400 mt-2">{listError}</p>}
          </div>
        )}
      </div>
    </div>
  )
}
