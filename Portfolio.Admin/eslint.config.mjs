// @ts-check
import withNuxt from './.nuxt/eslint.config.mjs'

export default withNuxt(
  {
    rules: {
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
      'comma-dangle': ['error', 'never']
    }
  }
)
