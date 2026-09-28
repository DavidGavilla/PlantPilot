import { useState } from 'react'
import type { FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { Leaf } from '@phosphor-icons/react'
import { useAuth } from '../context/AuthContext'
import { ApiError } from '../lib/apiClient'
import { Button } from '../components/ui/Button'
import { TextField } from '../components/ui/TextField'
import { FormAlert } from '../components/ui/FormAlert'

export function LoginPage() {
  const { login } = useAuth()
  const navigate = useNavigate()

  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [formError, setFormError] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setFormError(null)
    setIsSubmitting(true)

    try {
      await login({ email, password })
      navigate('/')
    } catch (error: unknown) {
      if (error instanceof ApiError && error.status === 401) {
        setFormError('Email o contrasena incorrectos.')
      } else {
        setFormError('No se pudo iniciar sesion. Intentalo de nuevo en unos segundos.')
      }
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main
      id="main-content"
      className="flex min-h-[calc(100dvh-64px)] items-center justify-center bg-zinc-50 px-4 py-12 sm:px-6 dark:bg-zinc-950"
    >
      <div className="w-full max-w-sm">
        <div className="mb-8 flex flex-col items-center gap-3 text-center">
          <Leaf weight="fill" aria-hidden="true" className="h-9 w-9 text-brand-600 dark:text-brand-400" />
          <h1 className="text-balance text-2xl font-semibold tracking-tight text-zinc-900 dark:text-zinc-50">
            Bienvenido de nuevo
          </h1>
          <p className="text-sm text-zinc-500 dark:text-zinc-400">
            Accede a tus plantas, fincas y zonas de riego.
          </p>
        </div>

        <div className="rounded-2xl border border-zinc-200 bg-white p-7 shadow-sm shadow-zinc-900/5 sm:p-8 dark:border-zinc-800 dark:bg-zinc-900">
          <form onSubmit={handleSubmit} noValidate className="flex flex-col gap-5">
            {formError && <FormAlert message={formError} />}

            <TextField
              label="Email"
              type="email"
              value={email}
              onChange={setEmail}
              autoComplete="email"
              required
            />
            <TextField
              label="Contrasena"
              type="password"
              value={password}
              onChange={setPassword}
              autoComplete="current-password"
              required
            />

            <Button type="submit" isLoading={isSubmitting} className="mt-1 w-full">
              Iniciar sesion
            </Button>
          </form>
        </div>

        <p className="mt-6 text-center text-sm text-zinc-500 dark:text-zinc-400">
          No tienes cuenta todavia?{' '}
          <Link to="/register" className="font-semibold text-brand-600 hover:underline dark:text-brand-400">
            Crea una gratis
          </Link>
        </p>
      </div>
    </main>
  )
}
