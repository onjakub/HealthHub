export function getApiBase(): string {
  // Prefer explicit env var
  const envBase = process.env.NEXT_PUBLIC_API_BASE
  if (envBase && envBase.trim().length > 0) {
    return envBase.replace(/\/$/, '')
  }

  // In browser: if running Next dev server on 3000, target backend on 5023; otherwise use relative
  if (typeof window !== 'undefined') {
    if (window.location.port === '3000') {
      return 'http://localhost:5023'
    }
    return ''
  }

  // On the server during dev/SSR, default to backend localhost:5023
  return 'http://localhost:5023'
}
