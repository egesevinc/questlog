import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { getFollowers, getFollowing } from '../api/social'
import { getProfile, type UserSummary, type UserProfile } from '../api/users'

export function FollowListPage({ mode }: { mode: 'followers' | 'following' }) {
  const { userId } = useParams<{ userId: string }>()
  const [users, setUsers] = useState<UserSummary[]>([])
  const [profile, setProfile] = useState<UserProfile | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!userId) return
    setLoading(true)
    const fetch = mode === 'followers' ? getFollowers(userId) : getFollowing(userId)
    Promise.all([fetch, getProfile(userId)])
      .then(([u, p]) => {
        setUsers(u)
        setProfile(p)
      })
      .catch(() => setError('Could not load this list.'))
      .finally(() => setLoading(false))
  }, [userId, mode])

  if (loading) return <p className="text-text-muted">Loading…</p>
  if (error) return <p className="text-red-400">{error}</p>

  return (
    <div className="max-w-lg">
      <p className="text-sm text-text-muted mb-1">
        <Link to={`/profiles/${userId}`} className="hover:text-text transition-colors">
          {profile?.username ?? 'Profile'}
        </Link>
      </p>
      <h1 className="text-2xl font-semibold text-text mb-6">
        {mode === 'followers' ? 'Followers' : 'Following'}
      </h1>

      {users.length === 0 ? (
        <p className="text-text-muted">
          {mode === 'followers' ? 'No followers yet.' : 'Not following anyone yet.'}
        </p>
      ) : (
        <div className="flex flex-col gap-2">
          {users.map((u) => (
            <Link
              key={u.id}
              to={`/profiles/${u.id}`}
              className="flex items-center gap-3 bg-surface border border-border rounded px-4 py-3 hover:border-accent transition-colors"
            >
              <div className="w-9 h-9 shrink-0 rounded-full bg-base border border-border overflow-hidden flex items-center justify-center">
                {u.avatarUrl ? (
                  <img src={u.avatarUrl} alt={u.username} className="w-full h-full object-cover" />
                ) : (
                  <span className="text-sm text-text-muted">{u.username[0]?.toUpperCase()}</span>
                )}
              </div>
              <span className="text-text">{u.username}</span>
            </Link>
          ))}
        </div>
      )}
    </div>
  )
}
