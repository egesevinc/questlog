import { Routes, Route } from 'react-router-dom'
import { Layout } from './components/Layout'
import { ProtectedRoute } from './components/ProtectedRoute'
import { HomePage } from './pages/HomePage'
import { LoginPage } from './pages/LoginPage'
import { RegisterPage } from './pages/RegisterPage'
import { GameSearchPage } from './pages/GameSearchPage'
import { GameDetailPage } from './pages/GameDetailPage'
import { MyLogsPage } from './pages/MyLogsPage'
import { ListsPage } from './pages/ListsPage'
import { ListDetailPage } from './pages/ListDetailPage'
import { ProfilePage } from './pages/ProfilePage'
import { UserSearchPage } from './pages/UserSearchPage'
import { LogDetailPage } from './pages/LogDetailPage'
import { NotificationsPage } from './pages/NotificationsPage'
import { YearInReviewPage } from './pages/YearInReviewPage'

function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route path="/games/:igdbId" element={<GameDetailPage />} />
        <Route path="/lists/:listId" element={<ListDetailPage />} />
        <Route path="/profiles/:userId" element={<ProfilePage />} />
        <Route path="/profiles/:userId/year-in-review" element={<YearInReviewPage />} />
        <Route path="/logs/:logId" element={<LogDetailPage />} />
        {/* Public: shows the landing page when logged out, the feed when logged in. */}
        <Route path="/" element={<HomePage />} />
        <Route element={<ProtectedRoute />}>
          <Route path="/search" element={<GameSearchPage />} />
          <Route path="/logs" element={<MyLogsPage />} />
          <Route path="/lists" element={<ListsPage />} />
          <Route path="/people" element={<UserSearchPage />} />
          <Route path="/notifications" element={<NotificationsPage />} />
        </Route>
      </Route>
    </Routes>
  )
}

export default App
