import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { getYearInReview, type YearInReview } from '../api/logs'
import { getProfile, type UserProfile } from '../api/users'

export function YearInReviewPage() {
  const { userId } = useParams<{ userId: string }>()
  const [data, setData] = useState<YearInReview | null>(null)
  const [profile, setProfile] = useState<UserProfile | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!userId) return
    setLoading(true)
    Promise.all([getYearInReview(userId), getProfile(userId)])
      .then(([d, p]) => {
        setData(d)
        setProfile(p)
      })
      .catch(() => setError('Could not load this year in review.'))
      .finally(() => setLoading(false))
  }, [userId])

  if (loading) return <p className="text-text-muted">Loading…</p>
  if (error || !data) return <p className="text-red-400">{error ?? 'Not found.'}</p>

  const empty = data.totalLogged === 0

  return (
    <div className="max-w-2xl">
      <p className="text-sm text-text-muted mb-1">
        {profile?.username ?? 'This player'}’s year in review
      </p>
      <h1 className="text-5xl font-bold text-accent tracking-tight mb-8">{data.year}</h1>

      {empty ? (
        <p className="text-text-muted">
          Nothing logged in {data.year} yet.{' '}
          <Link to={`/profiles/${userId}`} className="text-accent hover:underline">
            Back to profile
          </Link>
        </p>
      ) : (
        <>
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 mb-8">
            <BigStat value={data.totalLogged} label="games logged" />
            <BigStat value={data.completed} label="completed" />
            <BigStat value={data.totalHoursPlayed} label="hours" />
            <BigStat value={data.averageRating ?? '—'} label="avg rating" />
          </div>

          {data.topGenres.length > 0 && (
            <div className="mb-8">
              <p className="text-sm text-text-muted mb-2">Your genres</p>
              <div className="flex flex-wrap gap-2">
                {data.topGenres.map((g) => (
                  <span key={g.genre} className="text-sm bg-surface border border-border rounded px-3 py-1.5">
                    {g.genre} <span className="text-text-muted">({g.count})</span>
                  </span>
                ))}
              </div>
            </div>
          )}

          {data.topRated.length > 0 && (
            <div>
              <p className="text-sm text-text-muted mb-2">Your highest rated</p>
              <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
                {data.topRated.map((game) => (
                  <Link key={game.igdbId} to={`/games/${game.igdbId}`} className="group">
                    <div className="aspect-[3/4] bg-surface border border-border rounded overflow-hidden mb-2 group-hover:border-accent transition-colors relative">
                      {game.coverUrl ? (
                        <img src={game.coverUrl} alt={game.gameName} className="w-full h-full object-cover" />
                      ) : (
                        <div className="w-full h-full flex items-center justify-center text-text-muted text-xs px-2 text-center">
                          {game.gameName}
                        </div>
                      )}
                      {game.rating != null && (
                        <div className="absolute bottom-1 right-1 bg-base/90 text-accent text-xs font-semibold rounded px-1.5 py-0.5">
                          {game.rating}/10
                        </div>
                      )}
                    </div>
                    <p className="text-sm text-text truncate group-hover:text-accent transition-colors">
                      {game.gameName}
                    </p>
                  </Link>
                ))}
              </div>
            </div>
          )}
        </>
      )}
    </div>
  )
}

function BigStat({ value, label }: { value: string | number; label: string }) {
  return (
    <div className="bg-surface border border-border rounded p-4 text-center">
      <p className="text-3xl font-bold text-text">{value}</p>
      <p className="text-xs text-text-muted mt-1">{label}</p>
    </div>
  )
}
