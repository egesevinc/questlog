import { useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { searchUsers, type UserSummary } from '../api/users'

export function UserSearchPage() {
  const [query, setQuery] = useState('')
  const [results, setResults] = useState<UserSummary[]>([])
  const [loading, setLoading] = useState(false)
  const [searched, setSearched] = useState(false)

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    if (!query.trim()) return
    setLoading(true)
    try {
      const users = await searchUsers(query.trim())
      setResults(users)
      setSearched(true)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div>
      <h1 className="text-2xl font-semibold text-text mb-6">Find people</h1>
      <form onSubmit={handleSubmit} className="flex gap-2 mb-8">
        <input
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          placeholder="Search by username…"
          className="flex-1 bg-surface border border-border rounded px-3 py-2 text-text focus:outline-none focus:border-accent"
        />
        <button
          type="submit"
          disabled={loading}
          className="bg-accent text-base font-medium rounded px-4 py-2 hover:bg-accent-hover transition-colors disabled:opacity-50 cursor-pointer"
        >
          {loading ? 'Searching…' : 'Search'}
        </button>
      </form>

      {searched && results.length === 0 && (
        <p className="text-text-muted">No users found for "{query}".</p>
      )}

      <div className="flex flex-col gap-2">
        {results.map((u) => (
          <Link
            key={u.id}
            to={`/profiles/${u.id}`}
            className="bg-surface border border-border rounded px-4 py-3 hover:border-accent transition-colors text-text"
          >
            {u.username}
          </Link>
        ))}
      </div>
    </div>
  )
}
