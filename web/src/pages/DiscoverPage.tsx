import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { getTrendingGames, type TrendingGame } from '../api/games'
import { getGlobalFeed, type FeedItem } from '../api/social'
import { FeedItemCard } from '../components/FeedItemCard'

export function DiscoverPage() {
  const [trending, setTrending] = useState<TrendingGame[]>([])
  const [feed, setFeed] = useState<FeedItem[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    Promise.all([getTrendingGames(), getGlobalFeed()])
      .then(([t, f]) => {
        setTrending(t)
        setFeed(f)
      })
      .catch(() => {})
      .finally(() => setLoading(false))
  }, [])

  if (loading) return <p className="text-text-muted">Loading…</p>

  return (
    <div>
      <h1 className="text-2xl font-semibold text-text mb-6">Discover</h1>

      {trending.length > 0 && (
        <section className="mb-10">
          <h2 className="text-lg font-semibold text-text mb-4">Trending games</h2>
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-6 gap-4">
            {trending.map((game) => (
              <Link key={game.igdbId} to={`/games/${game.igdbId}`} className="group">
                <div className="aspect-[3/4] bg-surface border border-border rounded overflow-hidden mb-2 group-hover:border-accent transition-colors relative">
                  {game.coverUrl ? (
                    <img src={game.coverUrl} alt={game.name} className="w-full h-full object-cover" />
                  ) : (
                    <div className="w-full h-full flex items-center justify-center text-text-muted text-xs px-2 text-center">
                      {game.name}
                    </div>
                  )}
                  {game.averageRating != null && (
                    <div className="absolute bottom-1 right-1 bg-base/90 text-accent text-xs font-semibold rounded px-1.5 py-0.5">
                      {game.averageRating}
                    </div>
                  )}
                </div>
                <p className="text-sm text-text truncate group-hover:text-accent transition-colors">
                  {game.name}
                </p>
                <p className="text-xs text-text-muted">
                  {game.logCount} {game.logCount === 1 ? 'log' : 'logs'}
                </p>
              </Link>
            ))}
          </div>
        </section>
      )}

      <section>
        <h2 className="text-lg font-semibold text-text mb-4">Recent activity</h2>
        {feed.length === 0 ? (
          <p className="text-text-muted">No activity yet — be the first to log a game.</p>
        ) : (
          <div className="flex flex-col gap-3">
            {feed.map((item) => (
              <FeedItemCard key={item.logId} item={item} />
            ))}
          </div>
        )}
      </section>
    </div>
  )
}
