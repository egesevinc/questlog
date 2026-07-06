# Questlog 🎮

[![CI](https://github.com/egesevinc/questlog/actions/workflows/ci.yml/badge.svg)](https://github.com/egesevinc/questlog/actions/workflows/ci.yml)

**Letterboxd, but for games.** Log what you play, rate it, review it, and build lists — backed by a real game database and a clean, production-shaped .NET API.

> I'm a backend engineer, and I kept noticing that film lovers have Letterboxd, but gamers don't have an equivalent that feels good. So I built one as a hobby project — and used it as an excuse to build a properly-architected ASP.NET Core backend end to end: external API integration with caching, JWT auth, a thoughtful relational model, tests, and a one-command Docker setup.

---

## Why this exists / what it shows

This is a portfolio project, but it's built like a small real product rather than a tutorial. The interesting engineering is in the backend:

- **A real external integration done carefully.** Game data comes from [IGDB](https://www.igdb.com/) (Twitch's game database). IGDB authenticates via Twitch OAuth2, blocks browser-origin requests, and rate-limits to 4 req/sec — so the app uses a server-side integration layer that handles token fetch/refresh, throttles outbound calls, and **read-through caches** games into Postgres so repeat lookups never touch IGDB.
- **A data model shaped for games, not films.** A game isn't watched once — it's played, abandoned, replayed, backlogged, or run for 200 hours. The core `GameLog` entity captures status, rating, hours played, and start/finish dates, with one log per user per game.
- **Clean architecture.** Domain → Application → Infrastructure → API, with dependencies pointing inward. Services depend on interfaces, not on EF Core or HttpContext, which keeps the core testable.
- **The unglamorous essentials.** JWT auth, BCrypt password hashing, centralized exception handling, EF Core migrations, unit **and** HTTP-level integration tests, GitHub Actions CI, and a one-command Docker Compose stack.

---

## Architecture

```
┌─────────────┐     ┌──────────────────────────────────────────┐
│   Client    │────▶│  Questlog.Api  (controllers, auth, CORS)  │
│ (React SPA) │     └───────────────────┬──────────────────────┘
└─────────────┘                         │
                          ┌─────────────▼─────────────┐
                          │   Questlog.Application     │
                          │  (interfaces, DTOs,        │
                          │   service contracts)       │
                          └─────────────┬─────────────┘
                                        │
              ┌─────────────────────────▼──────────────────────────┐
              │            Questlog.Infrastructure                  │
              │  EF Core / Postgres · IGDB client · JWT · hashing   │
              └───────┬──────────────────────────────┬─────────────┘
                      │                               │
              ┌───────▼────────┐            ┌─────────▼──────────┐
              │   PostgreSQL   │            │   IGDB API (Twitch)│
              │ (incl. game    │            │  search + details  │
              │  cache)        │            └────────────────────┘
              └────────────────┘
```

**Dependency rule:** `Domain` knows nothing about anyone. `Application` defines the contracts (`IIgdbClient`, `IGameService`, `ICurrentUser`). `Infrastructure` implements them. `Api` wires it together. You can swap Postgres or IGDB without touching business logic.

---

## Tech stack

| Layer       | Choice                                   |
| ----------- | ---------------------------------------- |
| Language    | C# / .NET 8                              |
| API         | ASP.NET Core Web API                     |
| Data        | Entity Framework Core + PostgreSQL       |
| Auth        | JWT bearer + BCrypt password hashing     |
| Game data   | IGDB API (Twitch OAuth2)                 |
| Tests       | xUnit + FluentAssertions + NSubstitute + WebApplicationFactory |
| CI          | GitHub Actions (backend + frontend)      |
| Infra       | Docker + Docker Compose                  |
| Frontend    | React + Vite + TypeScript + Tailwind     |

---

## The IGDB integration (the interesting part)

IGDB has three properties that make a naive client fail, and the design addresses each:

1. **Twitch OAuth2, token expires.** `TwitchTokenProvider` fetches an app access token and caches it in memory until just before it expires, refreshing on demand. A `SemaphoreSlim` prevents a stampede of token requests when several calls arrive after expiry.
2. **No browser-origin requests (CORS) + 4 req/sec limit.** All IGDB traffic goes through the server. `IgdbClient` caps concurrent outbound calls with a semaphore to stay inside the rate limit.
3. **Repeated lookups are wasteful.** `GameService` is **read-through cached**: search results and game details are upserted into Postgres, and detail reads are served locally while fresh (7-day TTL), so the same game is never fetched twice in a row. This also unlocks relational queries IGDB can't answer — e.g. a user's top genres across everything they've logged.

Secrets (`ClientId` / `ClientSecret`) are never committed; they're read from .NET user-secrets or environment variables.

---

## Data model

```
User ──< GameLog >── Game >──< Genre
 │  │      │           └──< Platform
 │  │      └── Review (1:1)
 │  └──< GameList ──< GameListItem >── Game
 └──< Follow >── User            (self-referencing, follower → followee)
```

- **GameLog** — the core: `Status` (Wishlist/Backlog/Playing/Completed/Abandoned/Replaying), `Rating` (1–10, nullable), `HoursPlayed`, `StartedAt`, `FinishedAt`. Unique index on `(UserId, GameId)`.
- **Game** — a local cache of IGDB metadata with many-to-many genres and platforms.
- **Review** — one per log, anchored to a specific playthrough.
- **GameList / GameListItem** — ordered, curated lists.
- **Follow** — a directed follow edge between two users; unique on `(FollowerId, FolloweeId)`. Powers the activity feed.

---

## API surface

| Method | Route                                | Auth | Purpose                              |
| ------ | ------------------------------------ | ---- | ------------------------------------ |
| POST   | `/api/auth/register`                 | —    | Create an account                    |
| POST   | `/api/auth/login`                    | —    | Get a JWT                            |
| GET    | `/api/games/search?q=`               | —    | Search games (proxied + cached)      |
| GET    | `/api/games/{igdbId}`                | —    | Game details                         |
| POST   | `/api/logs`                          | ✅   | Log a game (with optional review)    |
| PUT    | `/api/logs/{id}`                     | ✅   | Update a log / its review            |
| DELETE | `/api/logs/{id}`                     | ✅   | Delete a log                         |
| GET    | `/api/logs/me`                       | ✅   | The current user's logs              |
| GET    | `/api/logs/{id}`                     | —    | A single log/review + comments       |
| POST   | `/api/logs/{id}/like`                | ✅   | Like a log / its review              |
| DELETE | `/api/logs/{id}/like`                | ✅   | Remove a like                        |
| POST   | `/api/logs/{id}/comments`            | ✅   | Comment on a log                     |
| DELETE | `/api/logs/comments/{commentId}`     | ✅   | Delete a comment                     |
| GET    | `/api/lists/me`                      | ✅   | The current user's lists             |
| POST   | `/api/lists`                         | ✅   | Create a list                        |
| GET    | `/api/lists/{id}`                    | —    | A list with its items                |
| PUT    | `/api/lists/{id}`                    | ✅   | Rename / edit a list                 |
| DELETE | `/api/lists/{id}`                    | ✅   | Delete a list                        |
| POST   | `/api/lists/{id}/items`              | ✅   | Add a game to a list                 |
| DELETE | `/api/lists/{id}/items/{itemId}`     | ✅   | Remove a game from a list            |
| PUT    | `/api/lists/{id}/items/order`        | ✅   | Reorder a list's items               |
| GET    | `/api/games/{igdbId}/community`      | —    | A game's avg rating + reviews        |
| GET    | `/api/profiles/search?q=`            | —    | Find users by username               |
| GET    | `/api/profiles/{userId}`             | —    | A user's public profile              |
| PUT    | `/api/profiles/me`                   | ✅   | Update your own bio / avatar         |
| GET    | `/api/profiles/{userId}/stats`       | —    | Aggregated taste stats               |
| GET    | `/api/profiles/{userId}/logs`        | —    | A user's public log grid             |
| GET    | `/api/profiles/{userId}/follow-info` | —    | Follower/following counts            |
| POST   | `/api/profiles/{userId}/follow`      | ✅   | Follow a user                        |
| DELETE | `/api/profiles/{userId}/follow`      | ✅   | Unfollow a user                      |
| GET    | `/api/feed`                          | ✅   | Activity from people you follow      |
| GET    | `/api/notifications`                 | ✅   | Your follow/like/comment notifications |
| GET    | `/api/notifications/unread-count`    | ✅   | Unread count (for the nav badge)     |
| POST   | `/api/notifications/read`            | ✅   | Mark all notifications read          |

Full interactive docs via Swagger at `/swagger` in development.

---

## Running it

### Option A — Docker (one command, full stack)

```bash
cp .env.example .env        # then fill in IGDB creds + a JWT secret
docker compose up --build
```

Brings up Postgres, the API, and the built React app together.

Web → http://localhost:5173  ·  API → http://localhost:8080  ·  Swagger → http://localhost:8080/swagger

### Option B — Local dev

```bash
# 1. Start Postgres (or use the compose db service)
# 2. Set secrets — never commit these:
cd src/Questlog.Api
dotnet user-secrets set "Igdb:ClientId" "<your-twitch-client-id>"
dotnet user-secrets set "Igdb:ClientSecret" "<your-twitch-client-secret>"
dotnet user-secrets set "Jwt:Secret" "<a-long-random-string>"

# 3. Apply migrations + run
dotnet ef database update --project ../Questlog.Infrastructure --startup-project .
dotnet run
```

Get IGDB credentials free at the [Twitch developer console](https://dev.twitch.tv/console).

### Tests

```bash
dotnet test        # unit tests + in-process HTTP integration tests
```

Both the backend (build + test) and frontend (typecheck + lint + build) run in
GitHub Actions on every push — see [`.github/workflows/ci.yml`](.github/workflows/ci.yml).

---

## What I'd build next

Treated as a roadmap, not a to-do — the v1 above is intentionally scoped to ship:

- **Real-time notifications** (push/websockets) — the notification data model and feed already ship (follow/like/comment); this would make the badge update live instead of on navigation.
- **Clips:** short vertical gameplay clips attached to logs — the long-term vision is a game-native feed where every clip is tied to the game it's from, which a generic video platform can't do.
- **Richer profiles:** year-in-review, "your gaming taste" summaries.
- **Caching upgrade:** move the token/game cache to Redis for multi-instance deploys.

---

## Notes

This is a personal project built for learning and as a portfolio piece. Game metadata is provided by IGDB under the Twitch Developer Service Agreement; Questlog is non-commercial.
