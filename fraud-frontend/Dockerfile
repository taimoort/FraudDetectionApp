# ---------- build stage ----------
FROM node:18-alpine AS builder
WORKDIR /app
COPY . .
RUN npm ci && npm run build   # creates production bundle in /app/dist

# ---------- serve stage ----------
FROM nginx:alpine
# custom Nginx site
COPY nginx.conf /etc/nginx/conf.d/default.conf
# copy static files into Nginx html directory
COPY --from=builder /app/dist /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]