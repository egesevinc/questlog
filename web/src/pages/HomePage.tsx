import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import { getFeed, getGlobalFeed, type FeedItem } from '../api/social'
import { getTrendingGames, type TrendingGame } from '../api/games'
import { FeedItemCard } from '../components/FeedItemCard'
import { LandingPage } from './LandingPage'

export function HomePage() {
  const { user } = useAuth()
  const [trending, setTrending] = useState<TrendingGame[]>([])
  const [feed, setFeed] = useState<FeedItem[]>([])
  const [followingActivity, setFollowingActivity] = useState(true)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    if (!user) {
      setLoading(false)
      return
    }
    Promise.all([getTrendingGames(6), getFeed()])
      .then(async ([t, following]) => {
        setTrending(t)
        if (following.length > 0) {
          setFeed(following)
          setFollowingActivity(true)
        } else {
          // Empty personal feed — fall back to recent activity from everyone.
          setFeed(await getGlobalFeed())
          setFollowingActivity(false)
        }
      })
      .catch(() => {})
      .finally(() => setLoading(false))
  }, [user])

  // Logged-out visitors get the marketing landing page instead of a login redirect.
  if (!user) return <LandingPage />

  return (
    <div>
      <h1 className="text-2xl font-semibold text-text mb-6">Welcome back, {user.username}</h1>

      {loading ? (
        <p className="text-text-muted">Loading…</p>
      ) : (
        <>
          {trending.length > 0 && (
            <section className="mb-10">
              <div className="flex items-center justify-between mb-4">
                <h2 className="text-lg font-semibold text-text">Trending games</h2>
                <Link to="/discover" className="text-sm text-accent hover:underline">
                  Discover more →
                </Link>
              </div>
              <div className="grid grid-cols-3 sm:grid-cols-6 gap-4">
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
                  </Link>
                ))}
              </div>
            </section>
          )}

          <section>
            <h2 className="text-lg font-semibold text-text mb-1">
              {followingActivity ? 'From people you follow' : 'Recent activity'}
            </h2>
            {!followingActivity && (
              <p className="text-sm text-text-muted mb-4">
                You're not following anyone yet —{' '}
                <Link to="/people" className="text-accent hover:underline">
                  find people to follow
                </Link>
                . Meanwhile, here's what the community is playing.
              </p>
            )}
            {followingActivity && <div className="mb-4" />}

            {feed.length === 0 ? (
              <p className="text-text-muted">
                No activity yet —{' '}
                <Link to="/search" className="text-accent hover:underline">
                  log your first game
                </Link>
                .
              </p>
            ) : (
              <div className="flex flex-col gap-3">
                {feed.map((item) => (
                  <FeedItemCard key={item.logId} item={item} />
                ))}
              </div>
            )}
          </section>
        </>
      )}
    </div>
  )
}
