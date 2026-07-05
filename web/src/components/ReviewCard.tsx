import { useState } from 'react'
import { Link } from 'react-router-dom'
import type { GameReview } from '../api/games'

/** A single community review, with spoiler bodies hidden behind a click. */
export function ReviewCard({ review }: { review: GameReview }) {
  const [revealed, setRevealed] = useState(false)
  const hidden = review.containsSpoilers && !revealed

  return (
    <div className="bg-surface border border-border rounded p-4">
      <div className="flex items-center justify-between mb-2">
        <div className="text-sm">
          <Link
            to={`/profiles/${review.userId}`}
            className="text-text font-medium hover:text-accent transition-colors"
          >
            {review.username}
          </Link>{' '}
          <span className="text-text-muted">· {review.status}</span>
        </div>
        {review.rating != null && (
          <span className="text-accent font-semibold text-sm">{review.rating}/10</span>
        )}
      </div>

      {hidden ? (
        <button
          onClick={() => setRevealed(true)}
          className="text-sm text-text-muted italic hover:text-text transition-colors cursor-pointer"
        >
          Contains spoilers — click to reveal
        </button>
      ) : (
        <p className="text-text text-sm leading-relaxed whitespace-pre-wrap">{review.body}</p>
      )}
    </div>
  )
}
