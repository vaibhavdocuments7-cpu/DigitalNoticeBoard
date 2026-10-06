import { useCallback, useEffect, useMemo, useState } from 'react'
import { apiRequest } from '../api'
import { AppNavbar } from '../components/AppNavbar'
import { StorageBadge } from '../components/StorageBadge'
import type { Notice, StorageInfo } from '../types'

export function BoardPage() {
  const [notices, setNotices] = useState<Notice[]>([])
  const [storage, setStorage] = useState<StorageInfo | null>(null)
  const [category, setCategory] = useState('All')
  const [activeIndex, setActiveIndex] = useState(0)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  const load = useCallback(async () => {
    try {
      const [published, storageInfo] = await Promise.all([
        apiRequest<Notice[]>('/notices/published'),
        apiRequest<StorageInfo>('/storage'),
      ])
      setNotices(published)
      setStorage(storageInfo)
      setError('')
    } catch (requestError) {
      setError(
        requestError instanceof Error
          ? requestError.message
          : 'Notices could not be loaded.',
      )
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    const initialLoadTimer = window.setTimeout(() => void load(), 0)
    const refreshTimer = window.setInterval(load, 30_000)
    return () => {
      window.clearTimeout(initialLoadTimer)
      window.clearInterval(refreshTimer)
    }
  }, [load])

  const categories = useMemo(
    () => ['All', ...new Set(notices.map((notice) => notice.category))],
    [notices],
  )
  const visibleNotices = useMemo(
    () =>
      category === 'All'
        ? notices
        : notices.filter((notice) => notice.category === category),
    [notices, category],
  )

  useEffect(() => {
    if (visibleNotices.length < 2) {
      return
    }

    const rotationTimer = window.setInterval(
      () => setActiveIndex((current) => (current + 1) % visibleNotices.length),
      7_000,
    )
    return () => window.clearInterval(rotationTimer)
  }, [visibleNotices.length])

  const activeNotice =
    visibleNotices.length > 0
      ? visibleNotices[activeIndex % visibleNotices.length]
      : null

  return (
    <>
      <AppNavbar />
      <main className="container-fluid board-page px-3 px-lg-5 py-4">
        <div className="d-flex flex-wrap justify-content-between align-items-center gap-3 mb-4">
          <div>
            <p className="text-primary fw-semibold mb-1">LIVE ANNOUNCEMENTS</p>
            <h1 className="h2 mb-0">What you need to know</h1>
          </div>
          <div className="d-flex align-items-center gap-3">
            <StorageBadge storage={storage} />
            <select
              className="form-select category-select"
              value={category}
              onChange={(event) => {
                setCategory(event.target.value)
                setActiveIndex(0)
              }}
              aria-label="Filter by category"
            >
              {categories.map((item) => (
                <option key={item}>{item}</option>
              ))}
            </select>
          </div>
        </div>

        {error && <div className="alert alert-danger">{error}</div>}
        {loading && (
          <div className="page-center">
            <div className="spinner-border text-primary" aria-label="Loading" />
          </div>
        )}

        {!loading && !activeNotice && (
          <div className="empty-state card border-0 shadow-sm">
            <div className="card-body text-center p-5">
              <h2 className="h4">No published notices</h2>
              <p className="text-secondary mb-0">
                Published announcements will appear here automatically.
              </p>
            </div>
          </div>
        )}

        {activeNotice && (
          <>
            <article className="featured-notice shadow-lg">
              <div className="featured-accent" />
              <div className="featured-content">
                <div className="d-flex flex-wrap gap-2 align-items-center mb-4">
                  <span className="badge text-bg-light text-primary">
                    {activeNotice.category}
                  </span>
                  <span className="badge priority-badge">
                    Priority {activeNotice.priority}
                  </span>
                </div>
                <h2 className="display-5 fw-bold">{activeNotice.title}</h2>
                <p className="lead mt-3 mb-4">{activeNotice.summary}</p>
                <p className="notice-content">{activeNotice.content}</p>
                <p className="small text-white-50 mt-5 mb-0">
                  Published{' '}
                  {new Date(activeNotice.publishFromUtc).toLocaleString()}
                  {activeNotice.publishUntilUtc &&
                    ` · Available until ${new Date(
                      activeNotice.publishUntilUtc,
                    ).toLocaleString()}`}
                </p>
              </div>
              <div className="slide-count">
                {activeIndex + 1} / {visibleNotices.length}
              </div>
            </article>

            <div className="row g-3 mt-2">
              {visibleNotices.map((notice, index) => (
                <div className="col-md-6 col-xl-4" key={notice.id}>
                  <button
                    className={`notice-preview card w-100 h-100 text-start border-0 shadow-sm ${
                      index === activeIndex ? 'active' : ''
                    }`}
                    type="button"
                    onClick={() => setActiveIndex(index)}
                  >
                    <div className="card-body">
                      <div className="d-flex justify-content-between mb-3">
                        <span className="badge text-bg-light">
                          {notice.category}
                        </span>
                        <span className="small text-secondary">
                          P{notice.priority}
                        </span>
                      </div>
                      <h3 className="h5">{notice.title}</h3>
                      <p className="text-secondary mb-0">{notice.summary}</p>
                    </div>
                  </button>
                </div>
              ))}
            </div>
          </>
        )}
      </main>
    </>
  )
}
