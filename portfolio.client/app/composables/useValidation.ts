export const useValidation = () => {
  const email = (v: string) =>
    /.+@.+\..+/.test(v) || 'Must be a valid email'

  const required = (v: unknown) =>
    !!v || 'Required'

  return {
    email,
    required
  }
}
