import { useState, type FormEvent } from 'react'
import { updateMyProfile, type UserProfile } from '../api/users'
import { getErrorMessage } from '../api/errors'

interface Props {
  profile: UserProfile
  onSaved: (updated: UserProfile) => void
  onCancel: () => void
}

export function ProfileEditForm({ profile, onSaved, onCancel }: Props) {
  const [bio, setBio] = useState(profile.bio ?? '')
  const [avatarUrl, setAvatarUrl] = useState(profile.avatarUrl ?? '')
  const [error, setError] = useState<string | null>(null)
  const [saving, setSaving] = useState(false)

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setError(null)
    setSaving(true)
    try {
      const updated = await updateMyProfile({
        bio: bio.trim() || null,
        avatarUrl: avatarUrl.trim() || null,
      })
      onSaved(updated)
    } catch (err) {
      setError(getErrorMessage(err, 'Could not save your profile.'))
    } finally {
      setSaving(false)
    }
  }

  return (
    <form onSubmit={handleSubmit} className="bg-surface border border-border rounded p-5 flex flex-col gap-4 mb-6">
      <div className="flex flex-col gap-1.5">
        <label className="text-sm text-text-muted">Bio</label>
        <textarea
          value={bio}
          onChange={(e) => setBio(e.target.value)}
          rows={3}
          maxLength={300}
          className="bg-base border border-border rounded px-3 py-2 text-text focus:outline-none focus:border-accent resize-none"
        />
      </div>
      <div className="flex flex-col gap-1.5">
        <label className="text-sm text-text-muted">Avatar URL</label>
        <input
          value={avatarUrl}
          onChange={(e) => setAvatarUrl(e.target.value)}
          placeholder="https://…"
          className="bg-base border border-border rounded px-3 py-2 text-text focus:outline-none focus:border-accent"
        />
      </div>
      {error && <p className="text-sm text-red-400">{error}</p>}
      <div className="flex gap-2">
        <button
          type="submit"
          disabled={saving}
          className="bg-accent text-base font-medium rounded px-4 py-2 hover:bg-accent-hover transition-colors disabled:opacity-50 cursor-pointer"
        >
          {saving ? 'Saving…' : 'Save profile'}
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
