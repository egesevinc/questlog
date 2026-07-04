import { useAuth } from '../auth/AuthContext'

export function HomePage() {
  const { user } = useAuth()

  return (
    <div>
      <h1 className="text-2xl font-semibold text-text mb-2">
        {user ? `Welcome back, ${user.username}` : 'Welcome to Questlog'}
      </h1>
      <p className="text-text-muted">
        Your gaming activity, logs, and lists will show up here.
      </p>
    </div>
  )
}
