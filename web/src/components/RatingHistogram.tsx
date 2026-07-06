/** A compact 1–10 rating distribution as vertical bars. */
export function RatingHistogram({ distribution }: { distribution: number[] }) {
  const max = Math.max(1, ...distribution)
  const total = distribution.reduce((a, b) => a + b, 0)

  if (total === 0) return null

  return (
    <div>
      <p className="text-sm text-text-muted mb-2">Rating distribution</p>
      <div className="flex items-end gap-1 h-24">
        {distribution.map((count, i) => (
          <div key={i} className="flex-1 flex flex-col items-center gap-1">
            <div className="w-full flex-1 flex items-end">
              <div
                className="w-full bg-accent/70 rounded-t transition-all"
                style={{ height: `${(count / max) * 100}%` }}
                title={`${i + 1}/10 — ${count} game${count === 1 ? '' : 's'}`}
              />
            </div>
            <span className="text-[10px] text-text-muted">{i + 1}</span>
          </div>
        ))}
      </div>
    </div>
  )
}
