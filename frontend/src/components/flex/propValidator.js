export const propValidator = (val, propName) => {
  const isValid = parseInt(val) > 0 && val <= 12
  if (!isValid) {
    console.warn(`Invalid prop value for '${propName}' in FlexColumn. Values must be between 1-12.`)
  }
  return isValid
}
