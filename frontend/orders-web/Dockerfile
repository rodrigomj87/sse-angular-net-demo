# Frontend Dockerfile (Angular build + Nginx serve)
FROM node:20-alpine AS build
WORKDIR /app
COPY frontend/orders-web/package*.json ./
RUN npm ci
COPY frontend/orders-web/ .
RUN npm run build -- --configuration production

FROM nginx:1.27-alpine AS final
# Copy built Angular dist (assuming default output path `dist/orders-web/browser` for Angular 17/18+)
COPY --from=build /app/dist/orders-web/browser /usr/share/nginx/html
# Basic Nginx config enabling gzip and increasing proxy buffers for SSE if later proxied
RUN sed -i 's/try_files \$uri \$uri\/ =404;/try_files \$uri \$uri\/ \/index.html;/' /etc/nginx/conf.d/default.conf \
    && echo '\nadd_header Cache-Control "no-cache";' >> /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
