import { useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { searchGames, type GameSummary } from '../api/games'

export function GameSearchPage() {
  const [query, setQuery] = useState('')
  const [results, setResults] = useState<GameSummary[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [searched, setSearched] = useState(false)

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    if (!query.trim()) return
    setError(null)
    setLoading(true)
    try {
      const games = await searchGames(query.trim())
      setResults(games)
      setSearched(true)
    } catch {
      setError('Search failed. Is the backend running?')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div>
      <h1 className="text-2xl font-semibold text-text mb-6">Find a game</h1>
      <form onSubmit={handleSubmit} className="flex gap-2 mb-8">
        <input
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          placeholder="Search by title…"
          className="flex-1 bg-surface border border-border rounded px-3 py-2 text-text focus:outline-none focus:border-accent"
        />
        <button
          type="submit"
          disabled={loading}
          className="bg-accent text-base font-medium rounded px-4 py-2 hover:bg-accent-hover transition-colors disabled:opacity-50 cursor-pointer"
        >
          {loading ? 'Searching…' : 'Search'}
        </button>
      </form>

      {error && <p className="text-sm text-red-400 mb-4">{error}</p>}

      {searched && !loading && results.length === 0 && !error && (
        <p className="text-text-muted">No games found for "{query}".</p>
      )}

      <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-5 gap-4">
        {results.map((game) => (
          <Link
            key={game.igdbId}
            to={`/games/${game.igdbId}`}
            className="group"
          >
            <div className="aspect-[3/4] bg-surface border border-border rounded overflow-hidden mb-2 group-hover:border-accent transition-colors">
              {game.coverUrl ? (
                <img src={game.coverUrl} alt={game.name} className="w-full h-full object-cover" />
              ) : (
                <div className="w-full h-full flex items-center justify-center text-text-muted text-xs px-2 text-center">
                  {game.name}
                </div>
              )}
            </div>
            <p className="text-sm text-text truncate group-hover:text-accent transition-colors">
              {game.name}
            </p>
          </Link>
        ))}
      </div>
    </div>
  )
}
