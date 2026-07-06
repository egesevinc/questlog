import { Link } from 'react-router-dom'
import type { FeedItem } from '../api/social'
import { LikeButton } from './LikeButton'

/** One activity-feed entry: who did what to which game, with like + comment actions. */
export function FeedItemCard({ item }: { item: FeedItem }) {
  return (
    <div className="flex gap-4 bg-surface border border-border rounded p-4">
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
          {item.rating != null && <span className="text-accent font-semibold"> · {item.rating}/10</span>}
        </p>
        {item.reviewBody && (
          <p className="text-sm text-text-muted mt-1 line-clamp-3 whitespace-pre-wrap">{item.reviewBody}</p>
        )}
        <div className="mt-2 flex items-center gap-4">
          <LikeButton logId={item.logId} initialCount={item.likeCount} initialLiked={item.likedByMe} />
          <Link to={`/logs/${item.logId}`} className="text-sm text-text-muted hover:text-text transition-colors">
            Comment
          </Link>
        </div>
      </div>
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
