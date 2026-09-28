import { useRef, useState } from 'react'
import type { FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { Leaf } from '@phosphor-icons/react'
import { useAuth } from '../context/AuthContext'
import { ApiError } from '../lib/apiClient'
import { Button } from '../components/ui/Button'
import { TextField } from '../components/ui/TextField'
import { FormAlert } from '../components/ui/FormAlert'

const PASSWORD_RULES = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,100}$/

interface FieldErrors {
  name?: string
  lastName?: string
  email?: string
  password?: string
}

function validate(name: string, lastName: string, email: string, password: string): FieldErrors {
  const errors: FieldErrors = {}
  if (!name.trim()) errors.name = 'Introduce tu nombre.'
  if (!lastName.trim()) errors.lastName = 'Introduce tus apellidos.'
  if (!/^\S+@\S+\.\S+$/.test(email)) errors.email = 'Introduce un email valido.'
  if (!PASSWORD_RULES.test(password)) {
    errors.password = 'Minimo 8 caracteres, con una mayuscula, una minuscula y un numero.'
  }
  return errors
}

export function RegisterPage() {
  const { register } = useAuth()
  const navigate = useNavigate()

  const [name, setName] = useState('')
  const [lastName, setLastName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [phoneNumber, setPhoneNumber] = useState('')
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({})
  const [formError, setFormError] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  const nameRef = useRef<HTMLInputElement>(null)
  const lastNameRef = useRef<HTMLInputElement>(null)
  const emailRef = useRef<HTMLInputElement>(null)
  const passwordRef = useRef<HTMLInputElement>(null)

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setFormError(null)

    const errors = validate(name, lastName, email, password)
    setFieldErrors(errors)
    if (Object.keys(errors).length > 0) {
      if (errors.name) nameRef.current?.focus()
      else if (errors.lastName) lastNameRef.current?.focus()
      else if (errors.email) emailRef.current?.focus()
      else if (errors.password) passwordRef.current?.focus()
      return
    }

    setIsSubmitting(true)
    try {
      await register({
        name: name.trim(),
        lastName: lastName.trim(),
        email: email.trim(),
        password,
        phoneNumber: phoneNumber.trim() || undefined,
      })
      navigate('/')
    } catch (error: unknown) {
      if (error instanceof ApiError && error.status === 409) {
        setFormError('Ya existe una cuenta con ese email.')
      } else if (error instanceof ApiError && error.status === 400) {
        setFormError(error.message)
      } else {
        setFormError('No se pudo crear la cuenta. Intentalo de nuevo en unos segundos.')
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
            Crea tu cuenta
          </h1>
          <p className="text-sm text-zinc-500 dark:text-zinc-400">
            Empieza con tu espacio personal, listo para plantas o fincas.
          </p>
        </div>

        <div className="rounded-2xl border border-zinc-200 bg-white p-7 shadow-sm shadow-zinc-900/5 sm:p-8 dark:border-zinc-800 dark:bg-zinc-900">
          <form onSubmit={handleSubmit} noValidate className="flex flex-col gap-5">
            {formError && <FormAlert message={formError} />}

            <div className="grid grid-cols-2 gap-4">
              <TextField
                ref={nameRef}
                label="Nombre"
                value={name}
                onChange={setName}
                autoComplete="given-name"
                error={fieldErrors.name}
                required
              />
              <TextField
                ref={lastNameRef}
                label="Apellidos"
                value={lastName}
                onChange={setLastName}
                autoComplete="family-name"
                error={fieldErrors.lastName}
                required
              />
            </div>

            <TextField
              ref={emailRef}
              label="Email"
              type="email"
              value={email}
              onChange={setEmail}
              autoComplete="email"
              error={fieldErrors.email}
              required
            />
            <TextField
              ref={passwordRef}
              label="Contrasena"
              type="password"
              value={password}
              onChange={setPassword}
              autoComplete="new-password"
              error={fieldErrors.password}
              helperText={
                fieldErrors.password ? undefined : 'Minimo 8 caracteres, con mayuscula, minuscula y numero.'
              }
              required
            />
            <TextField
              label="Telefono"
              type="tel"
              value={phoneNumber}
              onChange={setPhoneNumber}
              autoComplete="tel"
              helperText="Opcional."
            />

            <Button type="submit" isLoading={isSubmitting} className="mt-1 w-full">
              Crear cuenta
            </Button>
          </form>
        </div>

        <p className="mt-6 text-center text-sm text-zinc-500 dark:text-zinc-400">
          Ya tienes cuenta?{' '}
          <Link to="/login" className="font-semibold text-brand-600 hover:underline dark:text-brand-400">
            Inicia sesion
          </Link>
        </p>
      </div>
    </main>
  )
}
