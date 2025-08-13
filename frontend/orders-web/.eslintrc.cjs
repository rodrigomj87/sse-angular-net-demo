/* eslint-disable */
/**
 * @file: .eslintrc.cjs
 * @responsibility: ESLint configuration for Angular frontend
 */
module.exports = {
  root: true,
  ignorePatterns: ["dist", "node_modules"],
  plugins: [
    '@typescript-eslint',
    'import',
    'unused-imports',
    'prettier'
  ],
  extends: [
    'eslint:recommended',
    'plugin:@typescript-eslint/recommended',
    'plugin:import/recommended',
    'plugin:prettier/recommended'
  ],
  parser: '@typescript-eslint/parser',
  parserOptions: {
    ecmaVersion: 'latest',
    sourceType: 'module'
  },
  env: {
    browser: true,
    es2022: true,
    jasmine: true
  },
  rules: {
    'prettier/prettier': ['error'],
    'unused-imports/no-unused-imports': 'error',
    'import/order': [
      'warn',
      {
        'alphabetize': { order: 'asc', caseInsensitive: true },
        'newlines-between': 'always'
      }
    ],
    '@typescript-eslint/explicit-function-return-type': 'off',
    '@typescript-eslint/no-explicit-any': 'warn'
  }
};
