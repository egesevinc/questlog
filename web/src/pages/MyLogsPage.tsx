import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { getMyLogs, type GameLog } from '../api/logs'

export function MyLogsPage() {
  const [logs, setLogs] = useState<GameLog[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    getMyLogs()
      .then(setLogs)
      .catch(() => setError('Could not load your logs.'))
      .finally(() => setLoading(false))
  }, [])

  if (loading) return <p className="text-text-muted">Loading…</p>
  if (error) return <p className="text-red-400">{error}</p>

  return (
    <div>
      <h1 className="text-2xl font-semibold text-text mb-6">My logs</h1>
      {logs.length === 0 ? (
        <p className="text-text-muted">
          You haven't logged any games yet.{' '}
          <Link to="/search" className="text-accent hover:underline">
            Find one to log
          </Link>
          .
        </p>
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
              <p className="text-xs text-text-muted">{log.status}</p>
            </Link>
          ))}
        </div>
      )}
    </div>
  )
}
