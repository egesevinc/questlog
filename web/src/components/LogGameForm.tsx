import { useState, type FormEvent } from 'react'
import { createLog, type LogStatus } from '../api/logs'

const STATUSES: LogStatus[] = ['Wishlist', 'Backlog', 'Playing', 'Completed', 'Abandoned', 'Replaying']

interface Props {
  igdbId: number
  onSaved: () => void
  onCancel: () => void
}

export function LogGameForm({ igdbId, onSaved, onCancel }: Props) {
  const [status, setStatus] = useState<LogStatus>('Playing')
  const [rating, setRating] = useState('')
  const [hoursPlayed, setHoursPlayed] = useState('')
  const [reviewBody, setReviewBody] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [saving, setSaving] = useState(false)

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setError(null)
    setSaving(true)
    try {
      await createLog({
        igdbId,
        status,
        rating: rating ? Number(rating) : null,
        hoursPlayed: hoursPlayed ? Number(hoursPlayed) : null,
        startedAt: null,
        finishedAt: null,
        reviewBody: reviewBody.trim() || null,
      })
      onSaved()
    } catch {
      setError('Could not save the log.')
    } finally {
      setSaving(false)
    }
  }

  return (
    <form onSubmit={handleSubmit} className="bg-surface border border-border rounded p-5 flex flex-col gap-4">
      <div className="flex flex-col gap-1.5">
        <label className="text-sm text-text-muted">Status</label>
        <select
          value={status}
          onChange={(e) => setStatus(e.target.value as LogStatus)}
          className="bg-base border border-border rounded px-3 py-2 text-text focus:outline-none focus:border-accent"
        >
          {STATUSES.map((s) => (
            <option key={s} value={s}>
              {s}
            </option>
          ))}
        </select>
      </div>
      <div className="grid grid-cols-2 gap-4">
        <div className="flex flex-col gap-1.5">
          <label className="text-sm text-text-muted">Rating (1–10)</label>
          <input
            type="number"
            min={1}
            max={10}
            value={rating}
            onChange={(e) => setRating(e.target.value)}
            className="bg-base border border-border rounded px-3 py-2 text-text focus:outline-none focus:border-accent"
          />
        </div>
        <div className="flex flex-col gap-1.5">
          <label className="text-sm text-text-muted">Hours played</label>
          <input
            type="number"
            min={0}
            value={hoursPlayed}
            onChange={(e) => setHoursPlayed(e.target.value)}
            className="bg-base border border-border rounded px-3 py-2 text-text focus:outline-none focus:border-accent"
          />
        </div>
      </div>
      <div className="flex flex-col gap-1.5">
        <label className="text-sm text-text-muted">Review (optional)</label>
        <textarea
          value={reviewBody}
          onChange={(e) => setReviewBody(e.target.value)}
          rows={3}
          className="bg-base border border-border rounded px-3 py-2 text-text focus:outline-none focus:border-accent resize-none"
        />
      </div>
      {error && <p className="text-sm text-red-400">{error}</p>}
      <div className="flex gap-2">
        <button
          type="submit"
          disabled={saving}
          className="bg-accent text-base font-medium rounded px-4 py-2 hover:bg-accent-hover transition-colors disabled:opacity-50 cursor-pointer"
        >
          {saving ? 'Saving…' : 'Save log'}
        </button>
        <button
          type="button"
          onClick={onCancel}
          className="text-text-muted hover:text-text transition-colors px-4 py-2 cursor-pointer"
        >
          Cancel
        </button>
      </div>
    </form>
  )
}
