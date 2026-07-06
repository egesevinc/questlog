import { useEffect, useState } from 'react'
import { Link, Outlet, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import { getUnreadCount } from '../api/notifications'

export function Layout() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const [unread, setUnread] = useState(0)

  // Refresh the unread badge on login and on every navigation (so visiting the
  // notifications page, which marks all read, clears the badge on the way out).
  useEffect(() => {
    if (!user) {
      setUnread(0)
      return
    }
    getUnreadCount().then(setUnread).catch(() => {})
  }, [user, location.pathname])

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  return (
    <div className="min-h-screen flex flex-col">
      <header className="border-b border-border">
        <div className="max-w-5xl mx-auto flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 sm:gap-0 px-4 sm:px-6 py-4">
          <Link to="/" className="text-xl font-semibold text-text tracking-tight">
            Quest<span className="text-accent">log</span>
          </Link>
          <nav className="flex flex-wrap items-center gap-x-4 gap-y-2 text-sm">
            {user ? (
              <>
                <Link to="/discover" className="text-text-muted hover:text-text transition-colors">
                  Discover
                </Link>
                <Link to="/search" className="text-text-muted hover:text-text transition-colors">
                  Search
                </Link>
                <Link to="/logs" className="text-text-muted hover:text-text transition-colors">
                  My Logs
                </Link>
                <Link to="/lists" className="text-text-muted hover:text-text transition-colors">
                  Lists
                </Link>
                <Link to="/people" className="text-text-muted hover:text-text transition-colors">
                  People
                </Link>
                <Link
                  to="/notifications"
                  className="relative text-text-muted hover:text-text transition-colors"
                  title="Notifications"
                >
                  <span className="text-base leading-none">🔔</span>
                  {unread > 0 && (
                    <span className="absolute -top-2 -right-2 bg-accent text-base text-[10px] font-bold rounded-full min-w-4 h-4 px-1 flex items-center justify-center">
                      {unread > 9 ? '9+' : unread}
                    </span>
                  )}
                </Link>
                <Link to={`/profiles/${user.userId}`} className="text-text-muted hover:text-text transition-colors">
                  {user.username}
                </Link>
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
      <main className="flex-1 max-w-5xl w-full mx-auto px-4 sm:px-6 py-8 sm:py-10">
        <Outlet />
      </main>
    </div>
  )
}
