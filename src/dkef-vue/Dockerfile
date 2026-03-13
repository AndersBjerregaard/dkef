FROM oven/bun:1.3.10 AS build

ENV VITE_API_BASE_URL=https://api.andersbjerregaard.com
ENV VITE_MODE=k8s

WORKDIR /app

COPY package.json ./
COPY bun.lockb ./

RUN bun install --frozen-lockfile

COPY . .

RUN bun run build:k8s

FROM nginx:1.29.5-alpine AS final

RUN rm -rf /usr/share/nginx/html/*

COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD [ "nginx", "-g", "daemon off;" ]
