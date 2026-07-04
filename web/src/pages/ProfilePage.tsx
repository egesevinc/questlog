import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { getUserLogs, getUserStats, type GameLog, type ProfileStats } from '../api/logs'

export function ProfilePage() {
  const { userId } = useParams<{ userId: string }>()
  const [stats, setStats] = useState<ProfileStats | null>(null)
  const [logs, setLogs] = useState<GameLog[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!userId) return
    setLoading(true)
    Promise.all([getUserStats(userId), getUserLogs(userId)])
      .then(([s, l]) => {
        setStats(s)
        setLogs(l)
      })
      .catch(() => setError('Could not load this profile.'))
      .finally(() => setLoading(false))
  }, [userId])

  if (loading) return <p className="text-text-muted">Loading…</p>
  if (error || !stats) return <p className="text-red-400">{error ?? 'Profile not found.'}</p>

  return (
    <div>
      <h1 className="text-2xl font-semibold text-text mb-6">Profile</h1>

      <div className="grid grid-cols-2 sm:grid-cols-5 gap-4 mb-8">
        <Stat label="Logged" value={stats.totalLogged} />
        <Stat label="Completed" value={stats.completed} />
        <Stat label="Playing" value={stats.playing} />
        <Stat label="Backlog" value={stats.backlog} />
        <Stat label="Avg rating" value={stats.averageRating ?? '—'} />
      </div>

      {stats.topGenres.length > 0 && (
        <div className="mb-8">
          <p className="text-sm text-text-muted mb-2">Top genres</p>
          <div className="flex flex-wrap gap-2">
            {stats.topGenres.map((g) => (
              <span
                key={g.genre}
                className="text-sm bg-surface border border-border rounded px-3 py-1.5"
              >
                {g.genre} <span className="text-text-muted">({g.count})</span>
              </span>
            ))}
          </div>
        </div>
      )}

      <h2 className="text-lg font-semibold text-text mb-4">Logs</h2>
      {logs.length === 0 ? (
        <p className="text-text-muted">No logs yet.</p>
      ) : (
        <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-5 gap-4">
          {logs.map((log) => (
            <Link key={log.id} to={`/games/${log.igdbId}`} className="group">
              <div className="aspect-[3/4] bg-surface border border-border rounded overflow-hidden mb-2 group-hover:border-accent transition-colors relative">
                {log.coverUrl ? (
                  <img src={log.coverUrl} alt={log.gameName} className="w-full h-full object-cover" />
                ) : (
                  <div className="w-full h-full flex items-center justify-center text-text-muted text-xs px-2 text-center">
                    {log.gameName}
                  </div>
                )}
                {log.rating && (
                  <div className="absolute bottom-1 right-1 bg-base/90 text-accent text-xs font-semibold rounded px-1.5 py-0.5">
                    {log.rating}/10
                  </div>
                )}
              </div>
              <p className="text-sm text-text truncate group-hover:text-accent transition-colors">
                {log.gameName}
              </p>
            </Link>
          ))}
        </div>
      )}
    </div>
  )
}

function Stat({ label, value }: { label: string; value: string | number }) {
  return (
    <div className="bg-surface border border-border rounded px-4 py-3 text-center">
      <p className="text-xl font-semibold text-text">{value}</p>
      <p className="text-xs text-text-muted">{label}</p>
    </div>
  )
}
