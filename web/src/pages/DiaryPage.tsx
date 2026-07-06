import { useEffect, useMemo, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { getUserLogs, type GameLog } from '../api/logs'
import { getProfile, type UserProfile } from '../api/users'

export function DiaryPage() {
  const { userId } = useParams<{ userId: string }>()
  const [logs, setLogs] = useState<GameLog[]>([])
  const [profile, setProfile] = useState<UserProfile | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!userId) return
    setLoading(true)
    Promise.all([getUserLogs(userId), getProfile(userId)])
      .then(([l, p]) => {
        setLogs(l)
        setProfile(p)
      })
      .catch(() => setError('Could not load the diary.'))
      .finally(() => setLoading(false))
  }, [userId])

  // Group logs by "Month Year", preserving the newest-first order.
  const groups = useMemo(() => {
    const map = new Map<string, GameLog[]>()
    for (const log of logs) {
      const key = new Date(log.createdAt).toLocaleString('en-US', { month: 'long', year: 'numeric' })
      const bucket = map.get(key)
      if (bucket) bucket.push(log)
      else map.set(key, [log])
    }
    return [...map.entries()]
  }, [logs])

  if (loading) return <p className="text-text-muted">Loading…</p>
  if (error) return <p className="text-red-400">{error}</p>

  return (
    <div className="max-w-2xl">
      <p className="text-sm text-text-muted mb-1">{profile?.username ?? 'This player'}’s diary</p>
      <h1 className="text-2xl font-semibold text-text mb-8">Diary</h1>

      {logs.length === 0 ? (
        <p className="text-text-muted">No logs yet.</p>
      ) : (
        <div className="flex flex-col gap-8">
          {groups.map(([month, entries]) => (
            <div key={month}>
              <h2 className="text-sm font-semibold text-text-muted uppercase tracking-wide mb-3">
                {month}
              </h2>
              <div className="flex flex-col">
                {entries.map((log) => (
                  <Link
                    key={log.id}
                    to={`/logs/${log.id}`}
                    className="flex items-center gap-4 py-3 border-t border-border hover:bg-surface transition-colors -mx-2 px-2 rounded"
                  >
                    <span className="text-sm text-text-muted w-8 shrink-0 text-center">
                      {new Date(log.createdAt).getDate()}
                    </span>
                    <div className="w-8 aspect-[3/4] shrink-0 bg-surface border border-border rounded overflow-hidden">
                      {log.coverUrl ? (
                        <img src={log.coverUrl} alt={log.gameName} className="w-full h-full object-cover" />
                      ) : null}
                    </div>
                    <span className="text-text truncate flex-1">{log.gameName}</span>
                    <span className="text-xs text-text-muted shrink-0">{log.status}</span>
                    {log.rating != null && (
                      <span className="text-accent font-semibold text-sm shrink-0">{log.rating}/10</span>
                    )}
                  </Link>
                ))}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
