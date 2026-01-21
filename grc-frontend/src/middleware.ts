import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';

// Simplified middleware - locale detection without forced routing
// Full i18n routing can be enabled later with [locale] folder structure

export function middleware(request: NextRequest) {
  // For now, just pass through all requests
  // The app will use Arabic as default (set in layout.tsx)
  return NextResponse.next();
}

export const config = {
  matcher: [
    // Only match paths that might need locale handling
    // Skip static files, API routes, and Next.js internals
    '/((?!api|_next|_vercel|.*\\..*).*)',
  ],
};
