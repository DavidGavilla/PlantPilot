import { motion, useReducedMotion } from 'motion/react'
import { House, Buildings } from '@phosphor-icons/react'
import { TerrainContourVisual } from './TerrainContourVisual'

const revealTransition = { duration: 0.5, ease: [0.16, 1, 0.3, 1] as const }

export function AudienceSplitSection() {
  const reduce = useReducedMotion()

  return (
    <section className="mx-auto max-w-7xl px-4 py-20 sm:px-6">
      <h2 className="max-w-[38ch] text-balance text-3xl font-semibold tracking-tight text-zinc-900 sm:text-4xl dark:text-zinc-50">
        Pensada para quien cuida una planta y para quien gestiona cien hectareas.
      </h2>

      <div className="mt-10 grid gap-6 lg:grid-cols-2">
        <motion.div
          initial={reduce ? false : { opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true, amount: 0.3 }}
          transition={revealTransition}
          className="flex flex-col gap-4 overflow-hidden rounded-2xl border border-zinc-200 bg-white shadow-sm shadow-zinc-900/5 transition-shadow hover:shadow-md dark:border-zinc-800 dark:bg-zinc-900"
        >
          <div className="aspect-[16/9] w-full overflow-hidden bg-zinc-100 dark:bg-zinc-800">
            {/* Placeholder stock photography (Picsum, verified on-theme). */}
            <img
              src="https://picsum.photos/seed/aerial-farmland-07/720/405"
              alt="Planta en maceta, cuidada por un usuario particular"
              className="h-full w-full object-cover"
              width={720}
              height={405}
              loading="lazy"
            />
          </div>
          <div className="flex flex-col gap-3 p-8 pt-2">
            <House weight="duotone" aria-hidden="true" className="h-9 w-9 text-brand-600 dark:text-brand-400" />
            <h3 className="text-xl font-semibold text-zinc-900 dark:text-zinc-50">Particulares</h3>
            <p className="text-zinc-600 dark:text-zinc-400">
              Registra tus plantas, conoce sus necesidades y organiza el riego y los cuidados desde el movil o el
              ordenador.
            </p>
          </div>
        </motion.div>

        <motion.div
          initial={reduce ? false : { opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true, amount: 0.3 }}
          transition={{ ...revealTransition, delay: 0.08 }}
          className="flex flex-col gap-4 overflow-hidden rounded-2xl border border-zinc-200 bg-white shadow-sm shadow-zinc-900/5 transition-shadow hover:shadow-md dark:border-zinc-800 dark:bg-zinc-900"
        >
          <div className="aspect-[16/9] w-full overflow-hidden">
            <TerrainContourVisual />
          </div>
          <div className="flex flex-col gap-3 p-8 pt-2">
            <Buildings weight="duotone" aria-hidden="true" className="h-9 w-9 text-brand-600 dark:text-brand-400" />
            <h3 className="text-xl font-semibold text-zinc-900 dark:text-zinc-50">Empresas y profesionales</h3>
            <p className="text-zinc-600 dark:text-zinc-400">
              Organiza varias fincas, parcelas o zonas verdes, y sigue su estado con imagenes de satelite desde
              un unico sitio.
            </p>
          </div>
        </motion.div>
      </div>
    </section>
  )
}
