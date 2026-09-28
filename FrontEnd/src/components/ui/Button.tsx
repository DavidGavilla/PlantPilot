import type { ButtonHTMLAttributes, ReactNode } from 'react'

type Variant = 'primary' | 'secondary' | 'ghost'

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: Variant
  isLoading?: boolean
  children: ReactNode
}

const variantClasses: Record<Variant, string> = {
  primary:
    'bg-brand-600 text-white hover:bg-brand-700 focus-visible:ring-brand-500/50 dark:bg-brand-500 dark:hover:bg-brand-400 dark:text-brand-950',
  secondary:
    'bg-white text-zinc-900 border border-zinc-300 hover:bg-zinc-50 focus-visible:ring-zinc-400/50 dark:bg-zinc-900 dark:text-zinc-100 dark:border-zinc-700 dark:hover:bg-zinc-800',
  ghost:
    'bg-transparent text-zinc-700 hover:bg-zinc-100 focus-visible:ring-zinc-400/50 dark:text-zinc-300 dark:hover:bg-zinc-800',
}

export function Button({ variant = 'primary', isLoading, disabled, children, className, ...rest }: ButtonProps) {
  return (
    <button
      {...rest}
      disabled={disabled || isLoading}
      className={[
        'inline-flex items-center justify-center gap-2 whitespace-nowrap rounded-full px-5 py-2.5 text-sm font-semibold',
        'transition-[background-color,box-shadow,transform] duration-150',
        'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-offset-2 dark:focus-visible:ring-offset-zinc-950',
        'active:scale-[0.98] disabled:cursor-not-allowed disabled:opacity-60 disabled:active:scale-100',
        variantClasses[variant],
        className ?? '',
      ].join(' ')}
    >
      {isLoading && (
        <span
          aria-hidden="true"
          className="h-4 w-4 shrink-0 animate-spin rounded-full border-2 border-current border-t-transparent motion-reduce:animate-none"
        />
      )}
      {children}
    </button>
  )
}
