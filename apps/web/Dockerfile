FROM node:22-alpine AS deps
WORKDIR /app
COPY package.json package.json
COPY apps/web/package.json apps/web/package.json
RUN npm install

FROM node:22-alpine AS build
WORKDIR /app
ARG NEXT_PUBLIC_API_BASE_URL
ENV NEXT_PUBLIC_API_BASE_URL=$NEXT_PUBLIC_API_BASE_URL
COPY --from=deps /app/node_modules node_modules
COPY . .
RUN npm --workspace @meetsettle/web run build

FROM node:22-alpine AS runtime
WORKDIR /app
ENV NODE_ENV=production
COPY --from=build /app/package.json package.json
COPY --from=build /app/apps/web apps/web
COPY --from=build /app/node_modules node_modules
WORKDIR /app/apps/web
EXPOSE 3000
CMD ["npm", "run", "start"]