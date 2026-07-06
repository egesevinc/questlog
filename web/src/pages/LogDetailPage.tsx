import { useEffect, useState, type FormEvent } from 'react'
import { Link, useParams } from 'react-router-dom'
import { getLogDetail, addComment, deleteComment, type LogDetail } from '../api/logs'
import { LikeButton } from '../components/LikeButton'
import { ShareButton } from '../components/ShareButton'
import { useAuth } from '../auth/AuthContext'
import { getErrorMessage } from '../api/errors'

export function LogDetailPage() {
  const { logId } = useParams<{ logId: string }>()
  const { user } = useAuth()
  const [log, setLog] = useState<LogDetail | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [revealed, setRevealed] = useState(false)
  const [body, setBody] = useState('')
  const [commentError, setCommentError] = useState<string | null>(null)
  const [posting, setPosting] = useState(false)

  useEffect(() => {
    if (!logId) return
    setLoading(true)
    getLogDetail(logId)
      .then(setLog)
      .catch(() => setError('Could not load this log.'))
      .finally(() => setLoading(false))
  }, [logId])

  const handleAddComment = async (e: FormEvent) => {
    e.preventDefault()
    if (!logId || !body.trim()) return
    setCommentError(null)
    setPosting(true)
    try {
      const created = await addComment(logId, body.trim())
      setLog((prev) => (prev ? { ...prev, comments: [...prev.comments, created] } : prev))
      setBody('')
    } catch (err) {
      setCommentError(getErrorMessage(err, 'Could not post your comment.'))
    } finally {
      setPosting(false)
    }
  }

  const handleDeleteComment = async (commentId: string) => {
    await deleteComment(commentId)
    setLog((prev) =>
      prev ? { ...prev, comments: prev.comments.filter((c) => c.id !== commentId) } : prev,
    )
  }

  if (loading) return <p className="text-text-muted">Loading…</p>
  if (error || !log) return <p className="text-red-400">{error ?? 'Log not found.'}</p>

  const hidden = log.containsSpoilers && !revealed

  return (
    <div className="max-w-2xl">
      <div className="flex gap-4 mb-6">
        <Link to={`/games/${log.igdbId}`} className="shrink-0">
          <div className="w-24 aspect-[3/4] bg-surface border border-border rounded overflow-hidden">
            {log.coverUrl ? (
              <img src={log.coverUrl} alt={log.gameName} className="w-full h-full object-cover" />
            ) : null}
          </div>
        </Link>
        <div className="min-w-0">
          <Link
            to={`/games/${log.igdbId}`}
            className="text-xl font-semibold text-text hover:text-accent transition-colors"
          >
            {log.gameName}
          </Link>
          <p className="text-sm text-text-muted mt-1">
            <Link to={`/profiles/${log.userId}`} className="hover:text-text transition-colors">
              {log.username}
            </Link>{' '}
            · {log.status}
            {log.rating != null && <span className="text-accent font-semibold"> · {log.rating}/10</span>}
            {log.hoursPlayed != null && <span> · {log.hoursPlayed}h</span>}
          </p>
        </div>
      </div>

      {log.reviewBody && (
        <div className="mb-4">
          {hidden ? (
            <button
              onClick={() => setRevealed(true)}
              className="text-sm text-text-muted italic hover:text-text transition-colors cursor-pointer"
            >
              Contains spoilers — click to reveal
            </button>
          ) : (
            <p className="text-text leading-relaxed whitespace-pre-wrap">{log.reviewBody}</p>
          )}
        </div>
      )}

      <div className="mb-8 pb-6 border-b border-border flex items-center gap-4">
        <LikeButton logId={log.id} initialCount={log.likeCount} initialLiked={log.likedByMe} />
        <ShareButton sharePath={`logs/${log.id}`} />
      </div>

      <h2 className="text-lg font-semibold text-text mb-4">
        Comments <span className="text-text-muted font-normal">({log.comments.length})</span>
      </h2>

      {user && (
        <form onSubmit={handleAddComment} className="mb-6">
          <textarea
            value={body}
            onChange={(e) => setBody(e.target.value)}
            rows={2}
            placeholder="Add a comment…"
            className="w-full bg-surface border border-border rounded px-3 py-2 text-text focus:outline-none focus:border-accent resize-none mb-2"
          />
          {commentError && <p className="text-sm text-red-400 mb-2">{commentError}</p>}
          <button
            type="submit"
            disabled={posting || !body.trim()}
            className="bg-accent text-base font-medium rounded px-4 py-2 hover:bg-accent-hover transition-colors disabled:opacity-50 cursor-pointer"
          >
            {posting ? 'Posting…' : 'Comment'}
          </button>
        </form>
      )}

      {log.comments.length === 0 ? (
        <p className="text-text-muted text-sm">No comments yet.</p>
      ) : (
        <div className="flex flex-col gap-3">
          {log.comments.map((c) => (
            <div key={c.id} className="bg-surface border border-border rounded p-3">
              <div className="flex items-center justify-between mb-1">
                <Link
                  to={`/profiles/${c.userId}`}
                  className="text-sm font-medium text-text hover:text-accent transition-colors"
                >
                  {c.username}
                </Link>
                {(user?.userId === c.userId || user?.userId === log.userId) && (
                  <button
                    onClick={() => handleDeleteComment(c.id)}
                    className="text-xs text-text-muted hover:text-red-400 transition-colors cursor-pointer"
                  >
                    Delete
                  </button>
                )}
              </div>
              <p className="text-sm text-text whitespace-pre-wrap">{c.body}</p>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
