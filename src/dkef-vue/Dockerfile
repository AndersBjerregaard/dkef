FROM oven/bun:1.3.10 AS build

# Supported build modes: docker, k8s
ARG BUILD_MODE=k8s

WORKDIR /app

COPY package.json ./
COPY bun.lockb ./

RUN bun install --frozen-lockfile

COPY . .

RUN bun run build:${BUILD_MODE}

FROM nginx:1.29.5-alpine AS final

RUN rm -rf /usr/share/nginx/html/*

COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD [ "nginx", "-g", "daemon off;" ]
