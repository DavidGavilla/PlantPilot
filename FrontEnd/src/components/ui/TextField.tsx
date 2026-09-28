import { useId } from 'react'
import type { Ref } from 'react'

interface TextFieldProps {
  label: string
  name?: string
  type?: string
  value: string
  onChange: (value: string) => void
  autoComplete?: string
  placeholder?: string
  error?: string
  helperText?: string
  required?: boolean
  ref?: Ref<HTMLInputElement>
}

function slugify(label: string): string {
  return label
    .toLowerCase()
    .normalize('NFD')
    .replace(/[̀-ͯ]/g, '')
    .replace(/[^a-z0-9]+/g, '-')
}

export function TextField({
  label,
  name,
  type = 'text',
  value,
  onChange,
  autoComplete,
  placeholder,
  error,
  helperText,
  required,
  ref,
}: TextFieldProps) {
  const inputId = useId()
  const helperId = useId()
  const fieldName = name ?? slugify(label)

  return (
    <div className="flex flex-col gap-2">
      <label htmlFor={inputId} className="text-sm font-medium text-zinc-800 dark:text-zinc-200">
        {label}
        {required && <span aria-hidden="true" className="text-brand-600 dark:text-brand-400"> *</span>}
      </label>
      <input
        ref={ref}
        id={inputId}
        name={fieldName}
        type={type}
        value={value}
        onChange={(event) => onChange(event.target.value)}
        autoComplete={autoComplete}
        placeholder={placeholder}
        required={required}
        spellCheck={type === 'email' ? false : undefined}
        aria-invalid={Boolean(error)}
        aria-describedby={error || helperText ? helperId : undefined}
        className="w-full rounded-lg border border-zinc-300 bg-white px-3.5 py-2.5 text-zinc-900 placeholder:text-zinc-400 transition-colors focus-visible:border-brand-500 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-500/40 dark:border-zinc-700 dark:bg-zinc-900 dark:text-zinc-100 dark:placeholder:text-zinc-500 dark:focus-visible:border-brand-400 aria-invalid:border-red-500 aria-invalid:focus-visible:ring-red-500/40"
      />
      {error ? (
        <p id={helperId} className="text-sm text-red-600 dark:text-red-400">
          {error}
        </p>
      ) : helperText ? (
        <p id={helperId} className="text-sm text-zinc-500 dark:text-zinc-400">
          {helperText}
        </p>
      ) : null}
    </div>
  )
}
