import { useState, type FormEvent } from 'react'
import { searchGames, type GameSummary } from '../api/games'
import { setMyFavorites, type FavoriteGame } from '../api/users'
import { getErrorMessage } from '../api/errors'

interface Props {
  initial: FavoriteGame[]
  onSaved: (favorites: FavoriteGame[]) => void
  onCancel: () => void
}

interface Pick {
  igdbId: number
  gameName: string
  coverUrl: string | null
}

export function FavoritesEditor({ initial, onSaved, onCancel }: Props) {
  const [picks, setPicks] = useState<Pick[]>(
    initial.map((f) => ({ igdbId: f.igdbId, gameName: f.gameName, coverUrl: f.coverUrl })),
  )
  const [query, setQuery] = useState('')
  const [results, setResults] = useState<GameSummary[]>([])
  const [searching, setSearching] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [saving, setSaving] = useState(false)

  const full = picks.length >= 4

  const handleSearch = async (e: FormEvent) => {
    e.preventDefault()
    if (!query.trim()) return
    setSearching(true)
    try {
      setResults(await searchGames(query.trim()))
    } catch {
      setError('Search failed.')
    } finally {
      setSearching(false)
    }
  }

  const add = (g: GameSummary) => {
    if (full || picks.some((p) => p.igdbId === g.igdbId)) return
    setPicks([...picks, { igdbId: g.igdbId, gameName: g.name, coverUrl: g.coverUrl }])
  }

  const remove = (igdbId: number) => setPicks(picks.filter((p) => p.igdbId !== igdbId))

  const save = async () => {
    setError(null)
    setSaving(true)
    try {
      const saved = await setMyFavorites(picks.map((p) => p.igdbId))
      onSaved(saved)
    } catch (err) {
      setError(getErrorMessage(err, 'Could not save favourites.'))
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className="bg-surface border border-border rounded p-5 mb-6">
      <p className="text-sm text-text-muted mb-3">Pick up to 4 favourite games</p>

      {/* Current picks */}
      <div className="grid grid-cols-4 gap-3 mb-4">
        {[0, 1, 2, 3].map((slot) => {
          const pick = picks[slot]
          return (
            <div key={slot} className="relative">
              <div className="aspect-[3/4] bg-base border border-border rounded overflow-hidden flex items-center justify-center">
                {pick ? (
                  pick.coverUrl ? (
                    <img src={pick.coverUrl} alt={pick.gameName} className="w-full h-full object-cover" />
                  ) : (
                    <span className="text-xs text-text-muted px-1 text-center">{pick.gameName}</span>
                  )
                ) : (
                  <span className="text-2xl text-text-muted">+</span>
                )}
              </div>
              {pick && (
                <button
                  onClick={() => remove(pick.igdbId)}
                  className="absolute top-1 right-1 bg-base/80 text-text-muted hover:text-red-400 text-xs rounded px-1.5 cursor-pointer"
                >
                  ×
                </button>
              )}
            </div>
          )
        })}
      </div>

      {/* Search to add */}
      <form onSubmit={handleSearch} className="flex gap-2 mb-3">
        <input
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          placeholder={full ? 'Remove one to add more…' : 'Search a game to add…'}
          disabled={full}
          className="flex-1 bg-base border border-border rounded px-3 py-2 text-text focus:outline-none focus:border-accent disabled:opacity-50"
        />
        <button
          type="submit"
          disabled={searching || full}
          className="bg-base border border-border rounded px-4 py-2 text-text hover:border-accent transition-colors disabled:opacity-50 cursor-pointer"
        >
          Search
        </button>
      </form>

      {results.length > 0 && !full && (
        <div className="grid grid-cols-4 sm:grid-cols-6 gap-2 mb-4 max-h-48 overflow-y-auto">
          {results.map((g) => (
            <button
              key={g.igdbId}
              onClick={() => add(g)}
              disabled={picks.some((p) => p.igdbId === g.igdbId)}
              title={g.name}
              className="aspect-[3/4] bg-base border border-border rounded overflow-hidden hover:border-accent transition-colors disabled:opacity-40 cursor-pointer"
            >
              {g.coverUrl ? (
                <img src={g.coverUrl} alt={g.name} className="w-full h-full object-cover" />
              ) : (
                <span className="text-[10px] text-text-muted px-1">{g.name}</span>
              )}
            </button>
          ))}
        </div>
      )}

      {error && <p className="text-sm text-red-400 mb-2">{error}</p>}
      <div className="flex gap-2">
        <button
          onClick={save}
          disabled={saving}
          className="bg-accent text-base font-medium rounded px-4 py-2 hover:bg-accent-hover transition-colors disabled:opacity-50 cursor-pointer"
        >
          {saving ? 'Saving…' : 'Save favourites'}
        </button>
        <button
          onClick={onCancel}
          className="text-text-muted hover:text-text transition-colors px-4 py-2 cursor-pointer"
        >
          Cancel
        </button>
      </div>
    </div>
  )
}
