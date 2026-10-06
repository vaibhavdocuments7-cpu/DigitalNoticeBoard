import { useCallback, useEffect, useState, type FormEvent } from 'react'
import { apiRequest } from '../api'
import { AppNavbar } from '../components/AppNavbar'
import { StorageBadge } from '../components/StorageBadge'
import type {
  Notice,
  NoticeStatus,
  SaveNoticeRequest,
  StorageInfo,
} from '../types'

interface NoticeForm {
  title: string
  summary: string
  content: string
  category: string
  priority: number
  publishFrom: string
  publishUntil: string
  status: NoticeStatus
}

function toInputDate(value: Date | string) {
  const date = new Date(value)
  const offset = date.getTimezoneOffset()
  return new Date(date.getTime() - offset * 60_000).toISOString().slice(0, 16)
}

function createEmptyForm(): NoticeForm {
  const now = new Date()
  const nextWeek = new Date(now)
  nextWeek.setDate(nextWeek.getDate() + 7)
  return {
    title: '',
    summary: '',
    content: '',
    category: 'General',
    priority: 3,
    publishFrom: toInputDate(now),
    publishUntil: toInputDate(nextWeek),
    status: 'Draft',
  }
}

export function AdminPage() {
  const [notices, setNotices] = useState<Notice[]>([])
  const [storage, setStorage] = useState<StorageInfo | null>(null)
  const [form, setForm] = useState<NoticeForm>(createEmptyForm)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [message, setMessage] = useState('')

  const load = useCallback(async () => {
    try {
      const [allNotices, storageInfo] = await Promise.all([
        apiRequest<Notice[]>('/notices'),
        apiRequest<StorageInfo>('/storage'),
      ])
      setNotices(allNotices)
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
    return () => window.clearTimeout(initialLoadTimer)
  }, [load])

  const resetForm = () => {
    setForm(createEmptyForm())
    setEditingId(null)
  }

  const editNotice = (notice: Notice) => {
    setEditingId(notice.id)
    setForm({
      title: notice.title,
      summary: notice.summary,
      content: notice.content,
      category: notice.category,
      priority: notice.priority,
      publishFrom: toInputDate(notice.publishFromUtc),
      publishUntil: notice.publishUntilUtc
        ? toInputDate(notice.publishUntilUtc)
        : '',
      status: notice.status,
    })
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  const submit = async (event: FormEvent) => {
    event.preventDefault()
    setSaving(true)
    setError('')
    setMessage('')

    const request: SaveNoticeRequest = {
      title: form.title,
      summary: form.summary,
      content: form.content,
      category: form.category,
      priority: form.priority,
      publishFromUtc: new Date(form.publishFrom).toISOString(),
      publishUntilUtc: form.publishUntil
        ? new Date(form.publishUntil).toISOString()
        : null,
      status: form.status,
    }

    try {
      await apiRequest<Notice>(
        editingId ? `/notices/${editingId}` : '/notices',
        {
          method: editingId ? 'PUT' : 'POST',
          body: JSON.stringify(request),
        },
      )
      setMessage(editingId ? 'Notice updated.' : 'Notice created.')
      resetForm()
      await load()
    } catch (requestError) {
      setError(
        requestError instanceof Error ? requestError.message : 'Save failed.',
      )
    } finally {
      setSaving(false)
    }
  }

  const deleteNotice = async (notice: Notice) => {
    if (!window.confirm(`Delete "${notice.title}"?`)) {
      return
    }

    try {
      await apiRequest<void>(`/notices/${notice.id}`, { method: 'DELETE' })
      setMessage('Notice deleted.')
      await load()
    } catch (requestError) {
      setError(
        requestError instanceof Error ? requestError.message : 'Delete failed.',
      )
    }
  }

  return (
    <>
      <AppNavbar />
      <main className="container-fluid px-3 px-lg-5 py-4 admin-page">
        <div className="d-flex flex-wrap justify-content-between align-items-center gap-3 mb-4">
          <div>
            <p className="text-primary fw-semibold mb-1">ADMINISTRATION</p>
            <h1 className="h2 mb-0">Manage notices</h1>
          </div>
          <StorageBadge storage={storage} />
        </div>

        {storage?.isFallback && (
          <div className="alert alert-warning">
            <strong>Temporary storage:</strong> {storage.message} In-memory
            changes are lost when the API restarts.
          </div>
        )}
        {error && <div className="alert alert-danger">{error}</div>}
        {message && <div className="alert alert-success">{message}</div>}

        <div className="row g-4">
          <div className="col-xl-4">
            <div className="card border-0 shadow-sm sticky-xl-top admin-form-card">
              <div className="card-body p-4">
                <div className="d-flex justify-content-between align-items-center mb-3">
                  <h2 className="h4 mb-0">
                    {editingId ? 'Edit notice' : 'Create notice'}
                  </h2>
                  {editingId && (
                    <button
                      type="button"
                      className="btn btn-sm btn-outline-secondary"
                      onClick={resetForm}
                    >
                      Cancel
                    </button>
                  )}
                </div>

                <form onSubmit={submit}>
                  <div className="mb-3">
                    <label className="form-label" htmlFor="title">
                      Title
                    </label>
                    <input
                      id="title"
                      className="form-control"
                      maxLength={160}
                      value={form.title}
                      onChange={(event) =>
                        setForm({ ...form, title: event.target.value })
                      }
                      required
                    />
                  </div>
                  <div className="mb-3">
                    <label className="form-label" htmlFor="summary">
                      Summary
                    </label>
                    <textarea
                      id="summary"
                      className="form-control"
                      rows={2}
                      maxLength={500}
                      value={form.summary}
                      onChange={(event) =>
                        setForm({ ...form, summary: event.target.value })
                      }
                    />
                  </div>
                  <div className="mb-3">
                    <label className="form-label" htmlFor="content">
                      Content
                    </label>
                    <textarea
                      id="content"
                      className="form-control"
                      rows={5}
                      value={form.content}
                      onChange={(event) =>
                        setForm({ ...form, content: event.target.value })
                      }
                      required
                    />
                  </div>
                  <div className="row g-3 mb-3">
                    <div className="col-sm-7">
                      <label className="form-label" htmlFor="category">
                        Category
                      </label>
                      <input
                        id="category"
                        className="form-control"
                        value={form.category}
                        onChange={(event) =>
                          setForm({ ...form, category: event.target.value })
                        }
                      />
                    </div>
                    <div className="col-sm-5">
                      <label className="form-label" htmlFor="priority">
                        Priority
                      </label>
                      <select
                        id="priority"
                        className="form-select"
                        value={form.priority}
                        onChange={(event) =>
                          setForm({
                            ...form,
                            priority: Number(event.target.value),
                          })
                        }
                      >
                        {[1, 2, 3, 4, 5].map((priority) => (
                          <option key={priority}>{priority}</option>
                        ))}
                      </select>
                    </div>
                  </div>
                  <div className="mb-3">
                    <label className="form-label" htmlFor="publishFrom">
                      Publish from
                    </label>
                    <input
                      id="publishFrom"
                      className="form-control"
                      type="datetime-local"
                      value={form.publishFrom}
                      onChange={(event) =>
                        setForm({ ...form, publishFrom: event.target.value })
                      }
                      required
                    />
                  </div>
                  <div className="mb-3">
                    <label className="form-label" htmlFor="publishUntil">
                      Publish until
                    </label>
                    <input
                      id="publishUntil"
                      className="form-control"
                      type="datetime-local"
                      value={form.publishUntil}
                      onChange={(event) =>
                        setForm({ ...form, publishUntil: event.target.value })
                      }
                    />
                  </div>
                  <div className="mb-4">
                    <label className="form-label" htmlFor="status">
                      Status
                    </label>
                    <select
                      id="status"
                      className="form-select"
                      value={form.status}
                      onChange={(event) =>
                        setForm({
                          ...form,
                          status: event.target.value as NoticeStatus,
                        })
                      }
                    >
                      <option>Draft</option>
                      <option>Published</option>
                      <option>Archived</option>
                    </select>
                  </div>
                  <button
                    className="btn btn-primary w-100"
                    type="submit"
                    disabled={saving}
                  >
                    {saving
                      ? 'Saving...'
                      : editingId
                        ? 'Update notice'
                        : 'Create notice'}
                  </button>
                </form>
              </div>
            </div>
          </div>

          <div className="col-xl-8">
            <div className="card border-0 shadow-sm">
              <div className="card-body p-0">
                <div className="p-4 border-bottom">
                  <h2 className="h4 mb-1">All notices</h2>
                  <p className="text-secondary mb-0">
                    {notices.length} notice{notices.length === 1 ? '' : 's'}
                  </p>
                </div>
                {loading ? (
                  <div className="p-5 text-center">
                    <div
                      className="spinner-border text-primary"
                      aria-label="Loading"
                    />
                  </div>
                ) : (
                  <div className="table-responsive">
                    <table className="table table-hover align-middle mb-0">
                      <thead className="table-light">
                        <tr>
                          <th>Notice</th>
                          <th>Status</th>
                          <th>Schedule</th>
                          <th className="text-end">Actions</th>
                        </tr>
                      </thead>
                      <tbody>
                        {notices.map((notice) => (
                          <tr key={notice.id}>
                            <td>
                              <div className="fw-semibold">{notice.title}</div>
                              <div className="small text-secondary">
                                {notice.category} · Priority {notice.priority}
                              </div>
                            </td>
                            <td>
                              <span
                                className={`badge ${statusBadge(notice.status)}`}
                              >
                                {notice.status}
                              </span>
                            </td>
                            <td className="small">
                              <div>
                                {new Date(
                                  notice.publishFromUtc,
                                ).toLocaleString()}
                              </div>
                              <div className="text-secondary">
                                {notice.publishUntilUtc
                                  ? `to ${new Date(
                                      notice.publishUntilUtc,
                                    ).toLocaleString()}`
                                  : 'No expiry'}
                              </div>
                            </td>
                            <td className="text-end text-nowrap">
                              <button
                                className="btn btn-sm btn-outline-primary me-2"
                                type="button"
                                onClick={() => editNotice(notice)}
                              >
                                Edit
                              </button>
                              <button
                                className="btn btn-sm btn-outline-danger"
                                type="button"
                                onClick={() => void deleteNotice(notice)}
                              >
                                Delete
                              </button>
                            </td>
                          </tr>
                        ))}
                        {notices.length === 0 && (
                          <tr>
                            <td
                              className="text-center text-secondary p-5"
                              colSpan={4}
                            >
                              No notices have been created.
                            </td>
                          </tr>
                        )}
                      </tbody>
                    </table>
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      </main>
    </>
  )
}

function statusBadge(status: NoticeStatus) {
  switch (status) {
    case 'Published':
      return 'text-bg-success'
    case 'Archived':
      return 'text-bg-secondary'
    default:
      return 'text-bg-warning'
  }
}
