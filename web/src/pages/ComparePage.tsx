import { useEffect, useState, type FormEvent } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import {
  getGame,
  getGameCommunity,
  searchGames,
  type GameCommunity,
  type GameDetail,
  type GameSummary,
} from '../api/games'
import { getMyLogForGame, type GameLog } from '../api/logs'

interface Slot {
  game: GameDetail
  community: GameCommunity | null
  myLog: GameLog | null
}

type Side = 'a' | 'b'

/** Search box + cover grid for picking one game into a slot. */
function GamePicker({ label, onPick }: { label: string; onPick: (igdbId: number) => void }) {
  const [query, setQuery] = useState('')
  const [results, setResults] = useState<GameSummary[]>([])
  const [loading, setLoading] = useState(false)

  const search = async (e: FormEvent) => {
    e.preventDefault()
    if (!query.trim()) return
    setLoading(true)
    try {
      setResults(await searchGames(query.trim()))
    } catch {
      setResults([])
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="border border-dashed border-border rounded p-4 h-full">
      <p className="text-sm text-text-muted mb-3">{label}</p>
      <form onSubmit={search} className="flex gap-2 mb-3">
        <input
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          placeholder="Search a game…"
          className="flex-1 min-w-0 bg-surface border border-border rounded px-3 py-2 text-sm text-text focus:outline-none focus:border-accent"
        />
        <button
          type="submit"
          disabled={loading}
          className="bg-accent text-base text-sm font-medium rounded px-3 py-2 hover:bg-accent-hover transition-colors disabled:opacity-50 cursor-pointer"
        >
          {loading ? '…' : 'Search'}
        </button>
      </form>
      {results.length > 0 && (
        <div className="grid grid-cols-3 gap-2 max-h-64 overflow-y-auto">
          {results.map((g) => (
            <button
              key={g.igdbId}
              onClick={() => onPick(g.igdbId)}
              title={g.name}
              className="aspect-[3/4] bg-surface border border-border rounded overflow-hidden hover:border-accent transition-colors cursor-pointer"
            >
              {g.coverUrl ? (
                <img src={g.coverUrl} alt={g.name} className="w-full h-full object-cover" />
              ) : (
                <span className="text-[10px] text-text-muted px-1 block">{g.name}</span>
              )}
            </button>
          ))}
        </div>
      )}
    </div>
  )
}

/** The header card for a chosen game: cover, name, and a "change" control. */
function SlotHeader({ slot, onClear }: { slot: Slot; onClear: () => void }) {
  const { game } = slot
  return (
    <div className="border border-border rounded p-4 h-full">
      <div className="flex items-start justify-between gap-2 mb-3">
        <Link to={`/games/${game.igdbId}`} className="text-text font-medium hover:text-accent transition-colors">
          {game.name}
        </Link>
        <button
          onClick={onClear}
          className="shrink-0 text-xs text-text-muted hover:text-accent transition-colors cursor-pointer"
        >
          Change
        </button>
      </div>
      <Link to={`/games/${game.igdbId}`}>
        <div className="aspect-[3/4] bg-surface border border-border rounded overflow-hidden hover:border-accent transition-colors">
          {game.coverUrl ? (
            <img src={game.coverUrl} alt={game.name} className="w-full h-full object-cover" />
          ) : (
            <div className="w-full h-full flex items-center justify-center text-text-muted text-xs px-2 text-center">
              {game.name}
            </div>
          )}
        </div>
      </Link>
    </div>
  )
}

const year = (s: Slot) =>
  s.game.releaseDate ? String(new Date(s.game.releaseDate).getFullYear()) : '—'
const genres = (s: Slot) => (s.game.genres.length ? s.game.genres.join(', ') : '—')
const avg = (s: Slot) => s.community?.averageRating ?? null
const logs = (s: Slot) => s.community?.logCount ?? 0
const yourRating = (s: Slot) => s.myLog?.rating ?? null

export function ComparePage() {
  const [params, setParams] = useSearchParams()
  const [slotA, setSlotA] = useState<Slot | null>(null)
  const [slotB, setSlotB] = useState<Slot | null>(null)

  const loadSlot = async (igdbId: number): Promise<Slot | null> => {
    try {
      const [game, community, myLog] = await Promise.all([
        getGame(igdbId),
        getGameCommunity(igdbId).catch(() => null),
        getMyLogForGame(igdbId).catch(() => null),
      ])
      return { game, community, myLog }
    } catch {
      return null
    }
  }

  // Hydrate from the URL (?a=&b=) once, so comparisons are shareable / bookmarkable.
  useEffect(() => {
    const a = Number(params.get('a'))
    const b = Number(params.get('b'))
    if (a) loadSlot(a).then((s) => s && setSlotA(s))
    if (b) loadSlot(b).then((s) => s && setSlotB(s))
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const pick = async (side: Side, igdbId: number) => {
    const slot = await loadSlot(igdbId)
    if (!slot) return
    ;(side === 'a' ? setSlotA : setSlotB)(slot)
    const next = new URLSearchParams(params)
    next.set(side, String(igdbId))
    setParams(next, { replace: true })
  }

  const clear = (side: Side) => {
    ;(side === 'a' ? setSlotA : setSlotB)(null)
    const next = new URLSearchParams(params)
    next.delete(side)
    setParams(next, { replace: true })
  }

  const both = slotA && slotB

  // label + how to read each side's value, plus which side "wins" (higher is better).
  const rows = both
    ? [
        { label: 'Year', a: year(slotA), b: year(slotB) },
        { label: 'Genres', a: genres(slotA), b: genres(slotB) },
        {
          label: 'Community',
          a: avg(slotA) != null ? `${avg(slotA)!.toFixed(1)}/10` : '—',
          b: avg(slotB) != null ? `${avg(slotB)!.toFixed(1)}/10` : '—',
          win: compare(avg(slotA), avg(slotB)),
        },
        {
          label: 'Logged by',
          a: `${logs(slotA)}`,
          b: `${logs(slotB)}`,
          win: compare(logs(slotA), logs(slotB)),
        },
        {
          label: 'Your rating',
          a: yourRating(slotA) != null ? `${yourRating(slotA)}/10` : '—',
          b: yourRating(slotB) != null ? `${yourRating(slotB)}/10` : '—',
          win: compare(yourRating(slotA), yourRating(slotB)),
        },
        {
          label: 'Your status',
          a: slotA.myLog?.status ?? 'Not logged',
          b: slotB.myLog?.status ?? 'Not logged',
        },
      ]
    : []

  return (
    <div>
      <h1 className="text-2xl font-semibold text-text mb-1">Compare games</h1>
      <p className="text-text-muted mb-6">Pick two games to see them head to head.</p>

      <div className="grid grid-cols-2 gap-4 mb-8">
        {slotA ? (
          <SlotHeader slot={slotA} onClear={() => clear('a')} />
        ) : (
          <GamePicker label="First game" onPick={(id) => pick('a', id)} />
        )}
        {slotB ? (
          <SlotHeader slot={slotB} onClear={() => clear('b')} />
        ) : (
          <GamePicker label="Second game" onPick={(id) => pick('b', id)} />
        )}
      </div>

      {both ? (
        <div className="border border-border rounded overflow-hidden">
          {rows.map((row) => (
            <div key={row.label} className="grid grid-cols-[90px_1fr_1fr] sm:grid-cols-[130px_1fr_1fr] border-b border-border last:border-0">
              <div className="px-3 py-2.5 text-xs sm:text-sm text-text-muted bg-surface">{row.label}</div>
              <div className={`px-3 py-2.5 text-sm ${'win' in row && row.win === 'a' ? 'text-accent font-semibold' : 'text-text'}`}>
                {row.a}
              </div>
              <div className={`px-3 py-2.5 text-sm ${'win' in row && row.win === 'b' ? 'text-accent font-semibold' : 'text-text'}`}>
                {row.b}
              </div>
            </div>
          ))}
        </div>
      ) : (
        <p className="text-text-muted">Choose a game on each side to compare them.</p>
      )}
    </div>
  )
}

/** Returns which side is larger, or null on a tie / missing value. */
function compare(a: number | null, b: number | null): Side | null {
  if (a == null || b == null || a === b) return null
  return a > b ? 'a' : 'b'
}
