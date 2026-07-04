import { useEffect, useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { getMyLists, createList, type GameListSummary } from '../api/lists'

export function ListsPage() {
  const [lists, setLists] = useState<GameListSummary[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [title, setTitle] = useState('')
  const [creating, setCreating] = useState(false)

  const load = () => {
    setLoading(true)
    getMyLists()
      .then(setLists)
      .catch(() => setError('Could not load your lists.'))
      .finally(() => setLoading(false))
  }

  useEffect(load, [])

  const handleCreate = async (e: FormEvent) => {
    e.preventDefault()
    if (!title.trim()) return
    setCreating(true)
    try {
      await createList({ title: title.trim(), description: null, isPublic: true })
      setTitle('')
      load()
    } catch {
      setError('Could not create the list.')
    } finally {
      setCreating(false)
    }
  }

  return (
    <div>
      <h1 className="text-2xl font-semibold text-text mb-6">My lists</h1>

      <form onSubmit={handleCreate} className="flex gap-2 mb-8">
        <input
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          placeholder="New list title…"
          className="flex-1 bg-surface border border-border rounded px-3 py-2 text-text focus:outline-none focus:border-accent"
        />
        <button
          type="submit"
          disabled={creating}
          className="bg-accent text-base font-medium rounded px-4 py-2 hover:bg-accent-hover transition-colors disabled:opacity-50 cursor-pointer"
        >
          Create
        </button>
      </form>

      {error && <p className="text-sm text-red-400 mb-4">{error}</p>}
      {loading ? (
        <p className="text-text-muted">Loading…</p>
      ) : lists.length === 0 ? (
        <p className="text-text-muted">No lists yet — create your first one above.</p>
      ) : (
        <div className="flex flex-col gap-2">
          {lists.map((list) => (
            <Link
              key={list.id}
              to={`/lists/${list.id}`}
              className="bg-surface border border-border rounded px-4 py-3 hover:border-accent transition-colors flex justify-between items-center"
            >
              <div>
                <p className="text-text font-medium">{list.title}</p>
                {list.description && <p className="text-sm text-text-muted">{list.description}</p>}
              </div>
              <span className="text-sm text-text-muted">{list.itemCount} games</span>
            </Link>
          ))}
        </div>
      )}
    </div>
  )
}
