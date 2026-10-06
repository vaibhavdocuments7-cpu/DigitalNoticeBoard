import { NavLink, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'

export function AppNavbar() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()

  const signOut = () => {
    logout()
    navigate('/login')
  }

  return (
    <nav className="navbar navbar-expand-lg navbar-dark board-navbar shadow-sm">
      <div className="container-fluid px-lg-5">
        <NavLink className="navbar-brand fw-bold" to="/board">
          <span className="brand-mark">DNB</span>
          Digital Notice Board
        </NavLink>
        <div className="d-flex align-items-center gap-3">
          <NavLink className="nav-link text-white" to="/board">
            Board
          </NavLink>
          {user?.role === 'Admin' && (
            <NavLink className="nav-link text-white" to="/admin">
              Manage
            </NavLink>
          )}
          <span className="d-none d-md-inline text-white-50">
            {user?.displayName}
          </span>
          <button
            className="btn btn-sm btn-outline-light"
            type="button"
            onClick={signOut}
          >
            Sign out
          </button>
        </div>
      </div>
    </nav>
  )
}
