import { Link, Outlet, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'

export function Layout() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  return (
    <div className="min-h-screen flex flex-col">
      <header className="border-b border-border">
        <div className="max-w-5xl mx-auto flex items-center justify-between px-6 py-4">
          <Link to="/" className="text-xl font-semibold text-text tracking-tight">
            Quest<span className="text-accent">log</span>
          </Link>
          <nav className="flex items-center gap-4 text-sm">
            {user ? (
              <>
                <Link to="/search" className="text-text-muted hover:text-text transition-colors">
                  Search
                </Link>
                <Link to="/logs" className="text-text-muted hover:text-text transition-colors">
                  My Logs
                </Link>
                <Link to="/lists" className="text-text-muted hover:text-text transition-colors">
                  Lists
                </Link>
                <span className="text-text-muted">{user.username}</span>
                <button
                  onClick={handleLogout}
                  className="text-text-muted hover:text-text transition-colors cursor-pointer"
                >
                  Log out
                </button>
              </>
            ) : (
              <>
                <Link to="/login" className="text-text-muted hover:text-text transition-colors">
                  Log in
                </Link>
                <Link
                  to="/register"
                  className="bg-accent text-base px-3 py-1.5 rounded font-medium hover:bg-accent-hover transition-colors"
                >
                  Sign up
                </Link>
              </>
            )}
          </nav>
        </div>
      </header>
      <main className="flex-1 max-w-5xl w-full mx-auto px-6 py-10">
        <Outlet />
      </main>
    </div>
  )
}
