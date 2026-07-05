import { Link } from 'react-router-dom'

export function LandingPage() {
  return (
    <div>
      {/* Hero */}
      <section className="text-center py-12 sm:py-20">
        <h1 className="text-4xl sm:text-6xl font-bold text-text tracking-tight mb-4">
          Track the games <span className="text-accent">you play.</span>
        </h1>
        <p className="text-lg text-text-muted max-w-xl mx-auto mb-8">
          Questlog is Letterboxd for games — log what you play, rate and review it, build
          lists, and see what everyone else is playing.
        </p>
        <div className="flex items-center justify-center gap-3">
          <Link
            to="/register"
            className="bg-accent text-base font-semibold rounded px-6 py-2.5 hover:bg-accent-hover transition-colors"
          >
            Get started
          </Link>
          <Link
            to="/login"
            className="border border-border text-text rounded px-6 py-2.5 hover:border-accent transition-colors"
          >
            Log in
          </Link>
        </div>
      </section>

      {/* Feature highlights */}
      <section className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-12">
        <Feature
          title="Log & rate"
          body="Keep a diary of every game — status, rating, hours, and a review tied to the playthrough."
        />
        <Feature
          title="See the community"
          body="Every game has an average rating and reviews from everyone who's played it."
        />
        <Feature
          title="Follow & react"
          body="Follow friends, get an activity feed, and like or comment on their reviews."
        />
      </section>

      <p className="text-center text-xs text-text-muted">
        Game data by IGDB · a personal, non-commercial project.
      </p>
    </div>
  )
}

function Feature({ title, body }: { title: string; body: string }) {
  return (
    <div className="bg-surface border border-border rounded p-5">
      <h2 className="text-text font-semibold mb-2">{title}</h2>
      <p className="text-sm text-text-muted leading-relaxed">{body}</p>
    </div>
  )
}
