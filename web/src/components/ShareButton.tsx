import { useState } from 'react'
import { API_BASE } from '../api/client'

/**
 * Copies a rich-preview share link to the clipboard. The link points at the
 * API's /share/* route, which serves Open Graph tags so LinkedIn/Twitter show
 * a card, then redirects a human to the app.
 */
export function ShareButton({ sharePath }: { sharePath: string }) {
  const [copied, setCopied] = useState(false)

  const copy = async () => {
    const url = `${API_BASE.replace(/\/$/, '')}/share/${sharePath.replace(/^\//, '')}`
    try {
      await navigator.clipboard.writeText(url)
      setCopied(true)
      setTimeout(() => setCopied(false), 2000)
    } catch {
      // Clipboard blocked (e.g. insecure context) — fall back to a prompt.
      window.prompt('Copy this share link:', url)
    }
  }

  return (
    <button
      onClick={copy}
      className="text-sm text-text-muted hover:text-text transition-colors cursor-pointer"
      title="Copy a shareable link"
    >
      {copied ? 'Link copied ✓' : 'Share'}
    </button>
  )
}
