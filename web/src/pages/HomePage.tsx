import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import { getFeed, type FeedItem } from '../api/social'
import { FeedItemCard } from '../components/FeedItemCard'
import { LandingPage } from './LandingPage'

export function HomePage() {
  const { user } = useAuth()
  const [feed, setFeed] = useState<FeedItem[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    if (!user) {
      setLoading(false)
      return
    }
    getFeed()
      .then(setFeed)
      .catch(() => {})
      .finally(() => setLoading(false))
  }, [user])

  // Logged-out visitors get the marketing landing page instead of a login redirect.
  if (!user) return <LandingPage />

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
            <FeedItemCard key={item.logId} item={item} />
          ))}
        </div>
      )}
    </div>
  )
}
