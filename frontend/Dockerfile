# Align Node major with CI (quality.yml uses 20.19.x) — satisfies ESLint 10 engine constraint.
FROM node:20.19-alpine AS build
WORKDIR /app
COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci
COPY frontend/ ./
ARG VITE_API_URL=http://localhost:5230
ENV VITE_API_URL=${VITE_API_URL}
RUN npm run build

FROM nginx:alpine AS final
COPY --from=build /app/dist /usr/share/nginx/html
COPY frontend/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
HEALTHCHECK --interval=15s --timeout=5s --retries=5 \
    CMD wget -qO- http://127.0.0.1/ >/dev/null || exit 1
