// @ts-check

export default {
  '@stylistic/quote-props': 'off',
  indent: ['error', 2],
  'vue/html-indent': ['error', 2],
  semi: ['error', 'never'],
  'object-curly-spacing': ['error', 'always'],
  'space-before-function-paren': ['error', {
    anonymous: 'always',
    named: 'always',
    asyncArrow: 'always'
  }],
  quotes: ['error', 'single', { avoidEscape: true }],
  'no-trailing-spaces': 'error',
  'eol-last': ['error', 'always'],
  'vue/max-attributes-per-line': ['error', {
    singleline: {
      max: 1
    },
    multiline: {
      max: 1
    }
  }],
  'comma-dangle': ['error', 'never'],
  'vue/html-closing-bracket-newline': [
    'error', { singleline: 'never', multiline: 'never' }
  ]
}
