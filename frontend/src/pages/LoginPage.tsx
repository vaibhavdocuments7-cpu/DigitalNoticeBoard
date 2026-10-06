import { useEffect, useState, type FormEvent } from 'react'
import { Navigate, useNavigate } from 'react-router-dom'
import { apiRequest } from '../api'
import { useAuth } from '../auth/useAuth'
import { StorageBadge } from '../components/StorageBadge'
import type { StorageInfo } from '../types'

export function LoginPage() {
  const { user, login } = useAuth()
  const navigate = useNavigate()
  const [username, setUsername] = useState('viewer')
  const [password, setPassword] = useState('Viewer@123')
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const [storage, setStorage] = useState<StorageInfo | null>(null)

  useEffect(() => {
    apiRequest<StorageInfo>('/storage').then(setStorage).catch(() => undefined)
  }, [])

  if (user) {
    return <Navigate to={user.role === 'Admin' ? '/admin' : '/board'} replace />
  }

  const submit = async (event: FormEvent) => {
    event.preventDefault()
    setSubmitting(true)
    setError('')
    try {
      const signedInUser = await login(username, password)
      navigate(signedInUser.role === 'Admin' ? '/admin' : '/board')
    } catch (requestError) {
      setError(
        requestError instanceof Error ? requestError.message : 'Login failed.',
      )
    } finally {
      setSubmitting(false)
    }
  }

  const fillDemoAccount = (account: 'admin' | 'viewer') => {
    setUsername(account)
    setPassword(account === 'admin' ? 'Admin@123' : 'Viewer@123')
  }

  return (
    <main className="login-shell">
      <section className="login-copy">
        <div className="brand-mark brand-mark-large">DNB</div>
        <p className="text-uppercase tracking-label mb-3">Stay informed</p>
        <h1 className="display-4 fw-bold">
          One board for every important announcement.
        </h1>
        <p className="lead text-white-50 mt-4">
          Publish scheduled updates, highlight urgent news, and keep everyone
          aligned from a simple administration workspace.
        </p>
      </section>

      <section className="login-panel">
        <div className="card border-0 shadow-lg login-card">
          <div className="card-body p-4 p-lg-5">
            <div className="d-flex justify-content-between align-items-center mb-4">
              <div>
                <p className="text-primary fw-semibold mb-1">WELCOME BACK</p>
                <h2 className="h3 mb-0">Sign in</h2>
              </div>
              <StorageBadge storage={storage} />
            </div>

            {error && <div className="alert alert-danger">{error}</div>}

            <form onSubmit={submit}>
              <div className="mb-3">
                <label className="form-label" htmlFor="username">
                  Username
                </label>
                <input
                  id="username"
                  className="form-control form-control-lg"
                  value={username}
                  onChange={(event) => setUsername(event.target.value)}
                  autoComplete="username"
                  required
                />
              </div>
              <div className="mb-4">
                <label className="form-label" htmlFor="password">
                  Password
                </label>
                <input
                  id="password"
                  className="form-control form-control-lg"
                  type="password"
                  value={password}
                  onChange={(event) => setPassword(event.target.value)}
                  autoComplete="current-password"
                  required
                />
              </div>
              <button
                className="btn btn-primary btn-lg w-100"
                type="submit"
                disabled={submitting}
              >
                {submitting ? 'Signing in...' : 'Sign in to notice board'}
              </button>
            </form>

            <div className="demo-accounts mt-4">
              <p className="small text-secondary mb-2">Local demo accounts</p>
              <div className="d-flex gap-2">
                <button
                  className="btn btn-sm btn-outline-primary flex-fill"
                  type="button"
                  onClick={() => fillDemoAccount('viewer')}
                >
                  Viewer
                </button>
                <button
                  className="btn btn-sm btn-outline-dark flex-fill"
                  type="button"
                  onClick={() => fillDemoAccount('admin')}
                >
                  Administrator
                </button>
              </div>
            </div>
          </div>
        </div>
      </section>
    </main>
  )
}
