import { Drop, ChartLineUp, Plant } from '@phosphor-icons/react'
import { TerrainContourVisual } from './TerrainContourVisual'

export function CapabilitiesGrid() {
  return (
    <section className="mx-auto max-w-7xl px-4 py-20 sm:px-6">
      <h2 className="max-w-[34ch] text-balance text-3xl font-semibold tracking-tight text-zinc-900 sm:text-4xl dark:text-zinc-50">
        Todo lo necesario para cuidar y vigilar tu terreno.
      </h2>

      <div className="mt-10 grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">
        <div className="flex flex-col gap-4 rounded-2xl border border-zinc-200 bg-white p-8 shadow-sm shadow-zinc-900/5 transition-shadow hover:shadow-md sm:col-span-2 dark:border-zinc-800 dark:bg-zinc-900">
          <Plant weight="duotone" aria-hidden="true" className="h-8 w-8 text-brand-600 dark:text-brand-400" />
          <h3 className="text-lg font-semibold text-zinc-900 dark:text-zinc-50">Plantas, cultivos y parcelas</h3>
          <p className="text-zinc-600 dark:text-zinc-400">
            Organiza cada planta, cultivo o zona verde, desde una maceta hasta una parcela completa.
          </p>
        </div>

        <div className="flex flex-col gap-4 rounded-2xl border border-zinc-200 bg-white p-8 shadow-sm shadow-zinc-900/5 transition-shadow hover:shadow-md dark:border-zinc-800 dark:bg-zinc-900">
          <ChartLineUp weight="duotone" aria-hidden="true" className="h-8 w-8 text-brand-600 dark:text-brand-400" />
          <h3 className="text-lg font-semibold text-zinc-900 dark:text-zinc-50">Seguimiento del estado</h3>
          <p className="text-zinc-600 dark:text-zinc-400">Consulta el estado de cada planta y de cada zona en un vistazo.</p>
        </div>

        <div className="flex flex-col gap-4 rounded-2xl bg-brand-50 p-8 shadow-sm shadow-zinc-900/5 transition-shadow hover:shadow-md dark:bg-brand-950/40">
          <Drop weight="duotone" aria-hidden="true" className="h-8 w-8 text-brand-600 dark:text-brand-400" />
          <h3 className="text-lg font-semibold text-zinc-900 dark:text-zinc-50">Riego y planificacion</h3>
          <p className="text-zinc-700 dark:text-zinc-300">Programa el riego y otros cuidados sin depender de la memoria.</p>
        </div>

        <div className="relative h-72 overflow-hidden rounded-2xl sm:col-span-2 sm:h-80 lg:col-span-4">
          <div className="absolute inset-0">
            <TerrainContourVisual />
          </div>
          <div className="absolute inset-0 bg-gradient-to-t from-zinc-950/80 via-zinc-950/10 to-transparent" />
          <div className="absolute inset-x-0 bottom-0 p-8">
            <h3 className="text-xl font-semibold text-white">Analisis por satelite</h3>
            <p className="mt-2 max-w-[52ch] text-zinc-200">
              Visualiza el estado de la vegetacion en tus terrenos y detecta antes las zonas que necesitan
              atencion.
            </p>
          </div>
        </div>
      </div>
    </section>
  )
}
