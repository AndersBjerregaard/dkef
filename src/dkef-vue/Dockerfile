FROM oven/bun:1.2.18 AS build

ENV VITE_API_BASE_URL=http://localhost:5275
ENV VITE_MODE=Docker

WORKDIR /app

COPY package.json ./
COPY bun.lockb ./

RUN bun install --frozen-lockfile

COPY . .

RUN bun run build:docker

FROM nginx:1.29.0-alpine AS final

RUN rm -rf /usr/share/nginx/html/*

COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80

CMD [ "nginx", "-g", "daemon off;" ]