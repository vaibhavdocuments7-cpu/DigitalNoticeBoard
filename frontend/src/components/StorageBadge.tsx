import type { StorageInfo } from '../types'

export function StorageBadge({ storage }: { storage: StorageInfo | null }) {
  if (!storage) {
    return null
  }

  return (
    <span
      className={`badge rounded-pill ${
        storage.isFallback ? 'text-bg-warning' : 'text-bg-success'
      }`}
      title={storage.message}
    >
      {storage.provider}
    </span>
  )
}
