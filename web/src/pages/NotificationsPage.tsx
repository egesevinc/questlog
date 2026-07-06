import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { getNotifications, markAllRead, type Notification } from '../api/notifications'

export function NotificationsPage() {
  const [items, setItems] = useState<Notification[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    getNotifications()
      .then(setItems)
      .catch(() => {})
      .finally(() => setLoading(false))
    // Opening the page clears the unread badge.
    markAllRead().catch(() => {})
  }, [])

  if (loading) return <p className="text-text-muted">Loading…</p>

  return (
    <div>
      <h1 className="text-2xl font-semibold text-text mb-6">Notifications</h1>
      {items.length === 0 ? (
        <p className="text-text-muted">Nothing yet — follows, likes, and comments will show up here.</p>
      ) : (
        <div className="flex flex-col gap-2">
          {items.map((n) => (
            <NotificationRow key={n.id} n={n} />
          ))}
        </div>
      )}
    </div>
  )
}

function NotificationRow({ n }: { n: Notification }) {
  const actor = (
    <Link to={`/profiles/${n.actorId}`} className="font-medium text-text hover:text-accent transition-colors">
      {n.actorUsername}
    </Link>
  )

  let action: React.ReactNode
  let href = `/profiles/${n.actorId}`

  if (n.type === 'Follow') {
    action = <>started following you</>
  } else if (n.logId) {
    href = `/logs/${n.logId}`
    const game = n.gameName ? <span className="text-text">{n.gameName}</span> : <>your log</>
    action = n.type === 'Like' ? <>liked your review of {game}</> : <>commented on your review of {game}</>
  } else {
    action = <>interacted with you</>
  }

  return (
    <Link
      to={href}
      className={`block border border-border rounded px-4 py-3 hover:border-accent transition-colors ${
        n.isRead ? 'bg-surface' : 'bg-surface-hover'
      }`}
    >
      <p className="text-sm text-text-muted">
        {actor} <span className="text-text-muted">{action}</span>
      </p>
    </Link>
  )
}
