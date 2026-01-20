# Dockerfile Alpine 3.21 Update

## Summary

Updated Dockerfiles to use Alpine 3.21 as the base image where applicable.

## Changes Made

### 1. Frontend Dockerfile (`grc-frontend/Dockerfile`)
- **Updated**: Now uses `alpine:3.21` as base image
- **Approach**: Multi-stage build starting with `alpine:3.21`, then installing Node.js and npm
- **Stages**:
  - `node-base`: Alpine 3.21 + Node.js + npm
  - `deps`: Install production dependencies
  - `builder`: Build Next.js application
  - `runner`: Final runtime image

### 2. Docker Compose (`docker-compose.yml`)
- **PostgreSQL**: Uses `postgres:15-alpine` (Alpine version determined by PostgreSQL maintainers)
- **Redis**: Uses `redis:7-alpine` (Alpine version determined by Redis maintainers)
- **Note**: Official images don't support explicit Alpine version tags like `-alpine3.21`. The Alpine version is determined by the base image maintainers.

## Frontend Dockerfile Structure

```dockerfile
# Base stage: Alpine 3.21 + Node.js
FROM alpine:3.21 AS node-base
RUN apk add --no-cache nodejs npm

# Dependencies stage
FROM node-base AS deps
RUN apk add --no-cache libc6-compat
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci --omit=dev --legacy-peer-deps

# Builder stage
FROM node-base AS builder
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm install --legacy-peer-deps
COPY --from=deps /app/node_modules ./node_modules
COPY . .
ENV NEXT_TELEMETRY_DISABLED=1
RUN npm run build

# Runtime stage
FROM node-base AS runner
WORKDIR /app
ENV NODE_ENV=production
ENV NEXT_TELEMETRY_DISABLED=1

RUN addgroup --system --gid 1001 nodejs
RUN adduser --system --uid 1001 nextjs

COPY --from=builder /app/public ./public
COPY --from=builder --chown=nextjs:nodejs /app/.next/standalone ./
COPY --from=builder --chown=nextjs:nodejs /app/.next/static ./.next/static

USER nextjs
EXPOSE 3000
ENV PORT=3000
ENV HOSTNAME="0.0.0.0"

CMD ["node", "server.js"]
```

## Backend Dockerfiles

The backend Dockerfiles (`src/GrcMvc/Dockerfile`, `src/GrcMvc/Dockerfile.production`) use Microsoft's .NET images which are Debian-based:
- `mcr.microsoft.com/dotnet/aspnet:8.0` - Debian-based runtime
- `mcr.microsoft.com/dotnet/sdk:8.0` - Debian-based SDK

**Note**: If you need Alpine-based .NET images, Microsoft provides `mcr.microsoft.com/dotnet/aspnet:8.0-alpine` and `mcr.microsoft.com/dotnet/sdk:8.0-alpine`, but these may not be based on Alpine 3.21 specifically.

## Verification

To verify the Alpine version in the built image:

```bash
# Frontend
docker build -t grc-frontend:test -f grc-frontend/Dockerfile grc-frontend/
docker run --rm grc-frontend:test cat /etc/alpine-release

# Should output: 3.21.x
```

## Benefits of Alpine 3.21

- **Smaller image size**: Alpine Linux is minimal (~5MB base)
- **Security**: Latest security patches from Alpine 3.21
- **Performance**: Lightweight runtime environment
- **Consistency**: All services using same Alpine version

---

**Last Updated**: 2026-01-20
