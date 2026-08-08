# =========================
# BUILD STAGE
# =========================
FROM node:22 AS build

WORKDIR /app

COPY package*.json ./

RUN npm install

COPY . .

RUN npm run build

# =========================
# NGINX STAGE
# =========================
FROM nginx:alpine

COPY --from=build /app/dist/rms/browser /usr/share/nginx/html

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]