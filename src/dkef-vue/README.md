# !Important! caveat for `bun` users

Since `bun` version 1.3, `bun` loads `.env` files from the working directory.
Overwriting any vite specific environment or mode.
Meaning that if you run `bun run staging` with a `bun` version of 1.3 or above, you will get unintended results:
Actually loading the `.env.development` rather than `.env.staging`.
A workaround this current issue is to avoid using `package.json` scripts when working with vite modes.
For example, the direct alternative to the `package.json` script '`bun run staging`' is
`bun --env-file=.env.staging vite --mode staging`.

# tailwindvue

This template should help get you started developing with Vue 3 in Vite.

## Recommended IDE Setup

[VSCode](https://code.visualstudio.com/) + [Volar](https://marketplace.visualstudio.com/items?itemName=Vue.volar) (and disable Vetur).

## Type Support for `.vue` Imports in TS

TypeScript cannot handle type information for `.vue` imports by default, so we replace the `tsc` CLI with `vue-tsc` for type checking. In editors, we need [Volar](https://marketplace.visualstudio.com/items?itemName=Vue.volar) to make the TypeScript language service aware of `.vue` types.

## Customize configuration

See [Vite Configuration Reference](https://vite.dev/config/).

## Project Setup

```sh
bun install
```

### Compile and Hot-Reload for Development

```sh
bun dev
```

### Type-Check, Compile and Minify for Production

```sh
bun run build
```

### Build for specific environment

```sh
bun --env-file=.env.staging vite build --mode staging
```

### Preview compiled and built artifact

```sh
bun run preview
```

### Run Unit Tests with [Vitest](https://vitest.dev/)

```sh
bun test:unit
```

### Lint with [ESLint](https://eslint.org/)

```sh
bun lint
```
