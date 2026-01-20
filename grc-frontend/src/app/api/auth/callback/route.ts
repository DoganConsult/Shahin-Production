import { NextRequest, NextResponse } from 'next/server'

export async function GET(request: NextRequest) {
  const searchParams = request.nextUrl.searchParams
  const code = searchParams.get('code')
  const state = searchParams.get('state')
  const error = searchParams.get('error')

  // Handle OAuth2/OIDC callback from ABP OpenIddict
  if (error) {
    // Redirect to login page with error
    return NextResponse.redirect(new URL('/login?error=auth_failed', request.url))
  }

  if (code) {
    // Exchange authorization code for tokens
    try {
      const tokenResponse = await fetch('http://localhost:3006/connect/token', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: new URLSearchParams({
          grant_type: 'authorization_code',
          code: code,
          client_id: 'grc-web',
          redirect_uri: 'http://localhost:3003/api/auth/callback',
        }),
      })

      if (!tokenResponse.ok) {
        throw new Error('Token exchange failed')
      }

      const tokens = await tokenResponse.json()
      
      // Store tokens securely (in production, use httpOnly cookies)
      const response = NextResponse.redirect(new URL(state || '/dashboard', request.url))
      
      // Set secure httpOnly cookies for tokens
      response.cookies.set('access_token', tokens.access_token, {
        httpOnly: true,
        secure: process.env.NODE_ENV === 'production',
        sameSite: 'lax',
        maxAge: 60 * 60, // 1 hour
      })

      if (tokens.refresh_token) {
        response.cookies.set('refresh_token', tokens.refresh_token, {
          httpOnly: true,
          secure: process.env.NODE_ENV === 'production',
          sameSite: 'lax',
          maxAge: 60 * 60 * 24 * 30, // 30 days
        })
      }

      return response
    } catch (error) {
      console.error('OAuth callback error:', error)
      return NextResponse.redirect(new URL('/login?error=token_exchange_failed', request.url))
    }
  }

  // No code or error - redirect to login
  return NextResponse.redirect(new URL('/login', request.url))
}
