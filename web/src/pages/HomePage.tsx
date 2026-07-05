import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import { getFeed, type FeedItem } from '../api/social'
import { LikeButton } from '../components/LikeButton'

export function HomePage() {
  const { user } = useAuth()
  const [feed, setFeed] = useState<FeedItem[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    getFeed()
      .then(setFeed)
      .catch(() => {})
      .finally(() => setLoading(false))
  }, [])

  return (
    <div>
      <h1 className="text-2xl font-semibold text-text mb-1">
        {user ? `Welcome back, ${user.username}` : 'Welcome to Questlog'}
      </h1>
      <p className="text-text-muted mb-8">Recent activity from people you follow.</p>

      {loading ? (
        <p className="text-text-muted">Loading…</p>
      ) : feed.length === 0 ? (
        <p className="text-text-muted">
          Your feed is quiet.{' '}
          <Link to="/people" className="text-accent hover:underline">
            Find people to follow
          </Link>{' '}
          and their logs will show up here.
        </p>
      ) : (
        <div className="flex flex-col gap-3">
          {feed.map((item) => (
            <div
              key={item.logId}
              className="flex gap-4 bg-surface border border-border rounded p-4"
            >
              <Link to={`/games/${item.igdbId}`} className="shrink-0">
                <div className="w-16 aspect-[3/4] bg-base border border-border rounded overflow-hidden">
                  {item.coverUrl ? (
                    <img src={item.coverUrl} alt={item.gameName} className="w-full h-full object-cover" />
                  ) : null}
                </div>
              </Link>
              <div className="min-w-0">
                <p className="text-sm text-text">
                  <Link to={`/profiles/${item.userId}`} className="font-medium hover:text-accent transition-colors">
                    {item.username}
                  </Link>{' '}
                  <span className="text-text-muted">{statusVerb(item.status)}</span>{' '}
                  <Link to={`/games/${item.igdbId}`} className="font-medium hover:text-accent transition-colors">
                    {item.gameName}
                  </Link>
                  {item.rating != null && (
                    <span className="text-accent font-semibold"> · {item.rating}/10</span>
                  )}
                </p>
                {item.reviewBody && (
                  <p className="text-sm text-text-muted mt-1 line-clamp-3 whitespace-pre-wrap">
                    {item.reviewBody}
                  </p>
                )}
                <div className="mt-2">
                  <LikeButton
                    logId={item.logId}
                    initialCount={item.likeCount}
                    initialLiked={item.likedByMe}
                  />
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}

function statusVerb(status: FeedItem['status']): string {
  switch (status) {
    case 'Completed':
      return 'completed'
    case 'Playing':
      return 'is playing'
    case 'Abandoned':
      return 'abandoned'
    case 'Replaying':
      return 'is replaying'
    case 'Backlog':
      return 'backlogged'
    case 'Wishlist':
      return 'wishlisted'
    default:
      return 'logged'
  }
}
