# dkef-vue

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

### Run Unit Tests with [Vitest](https://vitest.dev/)

```sh
bun test:unit
```

### Lint with [ESLint](https://eslint.org/)

```sh
bun lint
```

## Tailwind Setup

### Install Tailwind CSS

Install `tailwindcss` via bun, and create your `tailwind.config.js` file.

```
bun install -D tailwindcss
bun tailwindcss init
```

### Configure template paths

Add the paths to all of your template files in your `tailwind.config.js` file

```
/** @type {import('tailwindcss').Config} */
export default {
  content: ["./src/**/*.{html,js,ts,vue}"],
  theme: {
    extend: {},
  },
  plugins: [],
}
```

### Add Tailwind directives to css

Add the `@tailwind` directives for each of Tailwind's layers to main css file. (`src/assets/main.css)

```
@tailwind base;
@tailwind components;
@tailwind utilities;
```

### Start Tailwind CLI build process

Run the CLI tool to scan your template files for classes and build your CSS. Notice this is a continuous process, and will consume your terminal session.

```
bun tailwindcss -i ./src/assets/main.css -o ./src/assets/output.css --watch
```
