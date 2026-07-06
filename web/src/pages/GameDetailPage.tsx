import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import { getGame, getGameCommunity, type GameDetail, type GameCommunity } from '../api/games'
import { getMyLists, addListItem, type GameListSummary } from '../api/lists'
import { getMyLogForGame, deleteLog, type GameLog } from '../api/logs'
import { useAuth } from '../auth/AuthContext'
import { LogGameForm } from '../components/LogGameForm'
import { ReviewCard } from '../components/ReviewCard'
import { ShareButton } from '../components/ShareButton'
import { getErrorMessage } from '../api/errors'

export function GameDetailPage() {
  const { igdbId } = useParams<{ igdbId: string }>()
  const { user } = useAuth()
  const [game, setGame] = useState<GameDetail | null>(null)
  const [loading, setLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)
  const [showLogForm, setShowLogForm] = useState(false)
  const [myLog, setMyLog] = useState<GameLog | null>(null)
  const [lists, setLists] = useState<GameListSummary[]>([])
  const [addedToList, setAddedToList] = useState<string | null>(null)
  const [listError, setListError] = useState<string | null>(null)
  const [community, setCommunity] = useState<GameCommunity | null>(null)

  useEffect(() => {
    if (!igdbId) return
    setLoading(true)
    getGame(Number(igdbId))
      .then(setGame)
      .catch(() => setLoadError('Could not load this game.'))
      .finally(() => setLoading(false))
    getGameCommunity(Number(igdbId)).then(setCommunity).catch(() => {})
  }, [igdbId])

  useEffect(() => {
    if (!user || !igdbId) return
    getMyLogForGame(Number(igdbId)).then(setMyLog).catch(() => {})
    getMyLists().then(setLists).catch(() => {})
  }, [user, igdbId])

  const reloadCommunity = () => {
    if (!igdbId) return
    getGameCommunity(Number(igdbId)).then(setCommunity).catch(() => {})
  }

  const reloadLog = () => {
    if (!igdbId) return
    getMyLogForGame(Number(igdbId)).then(setMyLog).catch(() => {})
    reloadCommunity()
  }

  const handleDeleteLog = async () => {
    if (!myLog) return
    await deleteLog(myLog.id)
    setMyLog(null)
    setShowLogForm(false)
    reloadCommunity()
  }

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
        <div className="flex items-start justify-between gap-4">
          <h1 className="text-2xl font-semibold text-text mb-1">{game.name}</h1>
          <div className="shrink-0 pt-1">
            <ShareButton sharePath={`games/${game.igdbId}`} />
          </div>
        </div>
        {game.releaseDate && (
          <p className="text-text-muted text-sm mb-4">{new Date(game.releaseDate).getFullYear()}</p>
        )}
        {game.genres.length > 0 && (
          <p className="text-sm text-text-muted mb-4">{game.genres.join(' · ')}</p>
        )}

        {community && community.ratingCount > 0 && (
          <div className="flex items-center gap-3 mb-4">
            <span className="text-2xl font-semibold text-accent">
              {community.averageRating?.toFixed(1)}
            </span>
            <span className="text-sm text-text-muted">
              average from {community.ratingCount}{' '}
              {community.ratingCount === 1 ? 'rating' : 'ratings'} · {community.logCount} logged
            </span>
          </div>
        )}

        {game.summary && <p className="text-text mb-6 leading-relaxed">{game.summary}</p>}

        {/* No log yet, not editing → offer to log it. */}
        {user && !myLog && !showLogForm && (
          <button
            onClick={() => setShowLogForm(true)}
            className="bg-accent text-base font-medium rounded px-4 py-2 hover:bg-accent-hover transition-colors cursor-pointer"
          >
            Log this game
          </button>
        )}

        {/* Existing log, not editing → show it with edit/delete. */}
        {user && myLog && !showLogForm && (
          <div className="bg-surface border border-border rounded p-4">
            <div className="flex items-center justify-between mb-2">
              <span className="text-text font-medium">{myLog.status}</span>
              {myLog.rating != null && (
                <span className="text-accent font-semibold text-sm">{myLog.rating}/10</span>
              )}
            </div>
            {myLog.hoursPlayed != null && (
              <p className="text-sm text-text-muted mb-2">{myLog.hoursPlayed}h played</p>
            )}
            {myLog.reviewBody && (
              <p className="text-text text-sm mb-3 leading-relaxed whitespace-pre-wrap">
                {myLog.reviewBody}
              </p>
            )}
            <div className="flex gap-2">
              <button
                onClick={() => setShowLogForm(true)}
                className="text-sm bg-base border border-border rounded px-3 py-1.5 hover:border-accent transition-colors cursor-pointer"
              >
                Edit
              </button>
              <button
                onClick={handleDeleteLog}
                className="text-sm text-text-muted hover:text-red-400 transition-colors px-3 py-1.5 cursor-pointer"
              >
                Delete
              </button>
            </div>
          </div>
        )}

        {showLogForm && (
          <LogGameForm
            igdbId={game.igdbId}
            existing={myLog}
            onSaved={() => {
              setShowLogForm(false)
              reloadLog()
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

        {community && community.reviews.length > 0 && (
          <div className="mt-8">
            <h2 className="text-lg font-semibold text-text mb-3">
              Reviews <span className="text-text-muted font-normal">({community.reviews.length})</span>
            </h2>
            <div className="flex flex-col gap-3">
              {community.reviews.map((review) => (
                <ReviewCard key={review.userId + review.createdAt} review={review} />
              ))}
            </div>
          </div>
        )}
      </div>
    </div>
  )
}
