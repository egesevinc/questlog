import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { getUserLogs, getUserStats, type GameLog, type ProfileStats } from '../api/logs'
import { getFollowInfo, followUser, unfollowUser, type FollowInfo } from '../api/social'
import { getProfile, type UserProfile } from '../api/users'
import { ProfileEditForm } from '../components/ProfileEditForm'
import { RatingHistogram } from '../components/RatingHistogram'
import { useAuth } from '../auth/AuthContext'

export function ProfilePage() {
  const { userId } = useParams<{ userId: string }>()
  const { user } = useAuth()
  const [profile, setProfile] = useState<UserProfile | null>(null)
  const [stats, setStats] = useState<ProfileStats | null>(null)
  const [logs, setLogs] = useState<GameLog[]>([])
  const [follow, setFollow] = useState<FollowInfo | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [editing, setEditing] = useState(false)

  const isMe = user?.userId === userId

  useEffect(() => {
    if (!userId) return
    setLoading(true)
    setEditing(false)
    Promise.all([getProfile(userId), getUserStats(userId), getUserLogs(userId), getFollowInfo(userId)])
      .then(([p, s, l, f]) => {
        setProfile(p)
        setStats(s)
        setLogs(l)
        setFollow(f)
      })
      .catch(() => setError('Could not load this profile.'))
      .finally(() => setLoading(false))
  }, [userId])

  const toggleFollow = async () => {
    if (!userId || !follow) return
    // Optimistic update, reverted on failure.
    const next = !follow.isFollowedByMe
    setFollow({
      ...follow,
      isFollowedByMe: next,
      followerCount: follow.followerCount + (next ? 1 : -1),
    })
    try {
      if (next) await followUser(userId)
      else await unfollowUser(userId)
    } catch {
      setFollow(follow) // revert
    }
  }

  if (loading) return <p className="text-text-muted">Loading…</p>
  if (error || !stats) return <p className="text-red-400">{error ?? 'Profile not found.'}</p>

  return (
    <div>
      <div className="flex items-start justify-between mb-4 gap-4">
        <div className="flex items-center gap-4 min-w-0">
          <div className="w-16 h-16 shrink-0 rounded-full bg-surface border border-border overflow-hidden flex items-center justify-center">
            {profile?.avatarUrl ? (
              <img src={profile.avatarUrl} alt={profile.username} className="w-full h-full object-cover" />
            ) : (
              <span className="text-2xl text-text-muted">
                {profile?.username?.[0]?.toUpperCase() ?? '?'}
              </span>
            )}
          </div>
          <div className="min-w-0">
            <h1 className="text-2xl font-semibold text-text truncate">
              {profile?.username ?? 'Profile'}
            </h1>
            {profile?.bio && <p className="text-sm text-text-muted mt-1">{profile.bio}</p>}
          </div>
        </div>
        {user && isMe && !editing && (
          <button
            onClick={() => setEditing(true)}
            className="text-sm border border-border rounded px-4 py-1.5 text-text-muted hover:text-text transition-colors cursor-pointer shrink-0"
          >
            Edit profile
          </button>
        )}
        {user && !isMe && follow && (
          <button
            onClick={toggleFollow}
            className={
              follow.isFollowedByMe
                ? 'text-sm border border-border rounded px-4 py-1.5 text-text-muted hover:text-text transition-colors cursor-pointer shrink-0'
                : 'text-sm bg-accent text-base rounded px-4 py-1.5 font-medium hover:bg-accent-hover transition-colors cursor-pointer shrink-0'
            }
          >
            {follow.isFollowedByMe ? 'Following' : 'Follow'}
          </button>
        )}
      </div>

      {editing && profile && (
        <ProfileEditForm
          profile={profile}
          onSaved={(updated) => {
            setProfile(updated)
            setEditing(false)
          }}
          onCancel={() => setEditing(false)}
        />
      )}

      {follow && (
        <div className="flex gap-6 mb-8 text-sm">
          <span className="text-text">
            <span className="font-semibold">{follow.followerCount}</span>{' '}
            <span className="text-text-muted">followers</span>
          </span>
          <span className="text-text">
            <span className="font-semibold">{follow.followingCount}</span>{' '}
            <span className="text-text-muted">following</span>
          </span>
        </div>
      )}

      <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-4 mb-8">
        <Stat label="Logged" value={stats.totalLogged} />
        <Stat label="Completed" value={stats.completed} />
        <Stat label="Playing" value={stats.playing} />
        <Stat label="Backlog" value={stats.backlog} />
        <Stat label="Avg rating" value={stats.averageRating ?? '—'} />
        <Stat label="Hours" value={stats.totalHoursPlayed} />
      </div>

      {stats.ratingDistribution.some((c) => c > 0) && (
        <div className="mb-8 max-w-md">
          <RatingHistogram distribution={stats.ratingDistribution} />
        </div>
      )}

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
