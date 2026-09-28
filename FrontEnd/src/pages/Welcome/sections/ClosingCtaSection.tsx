import { Link } from 'react-router-dom'

export function ClosingCtaSection() {
  return (
    <section className="mx-auto max-w-4xl px-4 py-24 text-center sm:px-6">
      <h2 className="text-3xl font-semibold tracking-tight text-zinc-900 sm:text-4xl dark:text-zinc-50">
        Empieza a gestionar tus plantas y tu terreno hoy.
      </h2>
      <p className="mx-auto mt-4 max-w-[42ch] text-zinc-600 dark:text-zinc-400">
        Crea tu cuenta y monta tu primer espacio en un par de minutos.
      </p>
      <Link
        to="/register"
        className="mt-8 inline-flex items-center justify-center whitespace-nowrap rounded-full bg-brand-600 px-7 py-3.5 text-sm font-semibold text-white shadow-sm shadow-brand-900/20 transition-[background-color,box-shadow,transform] duration-150 hover:bg-brand-700 hover:shadow-md focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-500/50 focus-visible:ring-offset-2 active:scale-[0.98] dark:bg-brand-500 dark:text-brand-950 dark:hover:bg-brand-400 dark:focus-visible:ring-offset-zinc-950"
      >
        Crear cuenta gratis
      </Link>
    </section>
  )
}
