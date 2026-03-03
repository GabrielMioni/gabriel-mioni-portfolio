export type Rule = (v: unknown) => true | string

export const required = (msg = 'Required'): Rule => (v) =>
  (v !== null && v !== undefined && String(v).trim().length > 0) || msg
