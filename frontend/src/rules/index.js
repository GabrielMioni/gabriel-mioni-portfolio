export const makeRequiredRules = (ruleType) => {
  return v => !!v || `${ruleType} is required`
}

export const urlRule = (value) => {
  const pattern = /^(https?:\/\/)?([\da-z.-]+)\.([a-z.]{2,6})([/\w .-]*)*\/?$/
  return pattern.test(value) || 'Invalid URL'
}
