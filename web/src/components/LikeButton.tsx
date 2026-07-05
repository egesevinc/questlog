import { useState } from 'react'
import { likeLog, unlikeLog } from '../api/social'
import { useAuth } from '../auth/AuthContext'

interface Props {
  logId: string
  initialCount: number
  initialLiked: boolean
}

/** Heart toggle with optimistic count; no-op affordance when logged out. */
export function LikeButton({ logId, initialCount, initialLiked }: Props) {
  const { user } = useAuth()
  const [liked, setLiked] = useState(initialLiked)
  const [count, setCount] = useState(initialCount)
  const [busy, setBusy] = useState(false)

  const toggle = async () => {
    if (!user || busy) return
    const next = !liked
    setLiked(next)
    setCount((c) => c + (next ? 1 : -1))
    setBusy(true)
    try {
      if (next) await likeLog(logId)
      else await unlikeLog(logId)
    } catch {
      // Revert on failure.
      setLiked(!next)
      setCount((c) => c + (next ? -1 : 1))
    } finally {
      setBusy(false)
    }
  }

  return (
    <button
      onClick={toggle}
      disabled={!user}
      title={user ? (liked ? 'Unlike' : 'Like') : 'Log in to like'}
      className={`inline-flex items-center gap-1 text-sm transition-colors ${
        user ? 'cursor-pointer' : 'cursor-default'
      } ${liked ? 'text-accent' : 'text-text-muted hover:text-text'}`}
    >
      <span>{liked ? '♥' : '♡'}</span>
      {count > 0 && <span>{count}</span>}
    </button>
  )
}
