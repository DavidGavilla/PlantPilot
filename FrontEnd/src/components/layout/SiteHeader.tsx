import { Link, useNavigate } from 'react-router-dom'
import { Leaf } from '@phosphor-icons/react'
import { useAuth } from '../../context/AuthContext'
import { Button } from '../ui/Button'

export function SiteHeader() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()

  const handleLogout = async () => {
    await logout()
    navigate('/')
  }

  return (
    <header className="sticky top-0 z-20 border-b border-zinc-200 bg-white/80 backdrop-blur-md dark:border-zinc-800 dark:bg-zinc-950/80">
      <div className="mx-auto flex h-16 max-w-7xl items-center justify-between px-4 sm:px-6">
        <Link
          to="/"
          className="flex items-center gap-2 rounded-md text-zinc-900 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-500/50 focus-visible:ring-offset-2 dark:text-zinc-50 dark:focus-visible:ring-offset-zinc-950"
        >
          <Leaf weight="fill" aria-hidden="true" className="h-6 w-6 text-brand-600 dark:text-brand-400" />
          <span className="text-base font-semibold tracking-tight">PlantPilot</span>
        </Link>

        {user ? (
          <div className="flex items-center gap-3">
            <span className="hidden text-sm text-zinc-600 sm:inline dark:text-zinc-400">
              Hola, {user.name}
            </span>
            <Button variant="secondary" onClick={handleLogout}>
              Cerrar sesion
            </Button>
          </div>
        ) : (
          <div className="flex items-center gap-3">
            <Link
              to="/login"
              className="rounded-full px-4 py-2 text-sm font-semibold text-zinc-700 transition-colors hover:text-zinc-900 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-500/50 focus-visible:ring-offset-2 dark:text-zinc-300 dark:hover:text-white dark:focus-visible:ring-offset-zinc-950"
            >
              Iniciar sesion
            </Link>
            <Link
              to="/register"
              className="inline-flex items-center justify-center whitespace-nowrap rounded-full bg-brand-600 px-5 py-2.5 text-sm font-semibold text-white shadow-sm shadow-brand-900/20 transition-[background-color,box-shadow,transform] duration-150 hover:bg-brand-700 hover:shadow-md focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-500/50 focus-visible:ring-offset-2 active:scale-[0.98] dark:bg-brand-500 dark:text-brand-950 dark:hover:bg-brand-400 dark:focus-visible:ring-offset-zinc-950"
            >
              Crear cuenta
            </Link>
          </div>
        )}
      </div>
    </header>
  )
}
