import { Link } from 'react-router-dom'
import { ArrowRight, CheckCircle } from '@phosphor-icons/react'
import { useAuth } from '../../../context/AuthContext'

export function HeroSection() {
  const { user } = useAuth()

  return (
    <section className="mx-auto grid max-w-7xl gap-10 px-4 pt-16 pb-20 sm:px-6 sm:pt-20 lg:grid-cols-2 lg:items-center lg:gap-16 lg:pt-24">
      <div className="flex flex-col gap-6">
        <h1 className="text-balance text-4xl font-semibold tracking-tight text-zinc-900 sm:text-5xl lg:text-6xl dark:text-zinc-50">
          De una planta en casa a una finca entera.
        </h1>
        <p className="max-w-[46ch] text-lg text-zinc-600 dark:text-zinc-400">
          Riego, seguimiento del estado y analisis por satelite en un mismo lugar, para particulares y equipos
          agricolas.
        </p>
        {user ? (
          <div className="flex items-center gap-2 text-sm font-medium text-brand-700 dark:text-brand-400">
            <CheckCircle weight="fill" aria-hidden="true" className="h-5 w-5" />
            Sesion iniciada como {user.name}.
          </div>
        ) : (
          <div className="flex flex-wrap items-center gap-4">
            <Link
              to="/register"
              className="inline-flex items-center justify-center gap-2 whitespace-nowrap rounded-full bg-brand-600 px-6 py-3 text-sm font-semibold text-white shadow-sm shadow-brand-900/20 transition-[background-color,box-shadow,transform] duration-150 hover:bg-brand-700 hover:shadow-md focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-500/50 focus-visible:ring-offset-2 active:scale-[0.98] dark:bg-brand-500 dark:text-brand-950 dark:hover:bg-brand-400 dark:focus-visible:ring-offset-zinc-950"
            >
              Crear cuenta gratis
              <ArrowRight weight="bold" aria-hidden="true" className="h-4 w-4" />
            </Link>
            <Link
              to="/login"
              className="inline-flex items-center justify-center whitespace-nowrap rounded-full border border-zinc-300 px-6 py-3 text-sm font-semibold text-zinc-800 transition-colors hover:bg-zinc-50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-500/50 focus-visible:ring-offset-2 dark:border-zinc-700 dark:text-zinc-200 dark:hover:bg-zinc-900 dark:focus-visible:ring-offset-zinc-950"
            >
              Iniciar sesion
            </Link>
          </div>
        )}
      </div>

      <div className="relative aspect-[4/5] w-full overflow-hidden rounded-3xl bg-zinc-100 lg:aspect-[5/6] dark:bg-zinc-900">
        {/* Placeholder stock photography (Picsum, verified on-theme) - swap for real branded photography before launch. */}
        <img
          src="https://picsum.photos/seed/greenhouse-rows-11/900/1080"
          alt="Vegetacion y terreno gestionados desde PlantPilot"
          className="h-full w-full object-cover"
          width={900}
          height={1080}
          loading="eager"
        />
      </div>
    </section>
  )
}
