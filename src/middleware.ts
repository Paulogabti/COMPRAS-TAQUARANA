import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';
import { jwtVerify } from 'jose';

const publicRoutes = ['/login'];

export async function middleware(req: NextRequest) {
  const { pathname } = req.nextUrl;
  if (pathname.startsWith('/api/auth/login')) return NextResponse.next();
  if (pathname.startsWith('/_next')) return NextResponse.next();

  const isPublic = publicRoutes.includes(pathname);
  const token = req.cookies.get('compras_session')?.value;

  if (!token && !isPublic) {
    return NextResponse.redirect(new URL('/login', req.url));
  }

  if (token && pathname === '/login') {
    return NextResponse.redirect(new URL('/', req.url));
  }

  if (token) {
    try {
      const secret = process.env.JWT_SECRET;
      if (!secret) throw new Error('JWT_SECRET não configurado');
      await jwtVerify(token, new TextEncoder().encode(secret));
    } catch {
      const res = NextResponse.redirect(new URL('/login', req.url));
      res.cookies.delete('compras_session');
      return res;
    }
  }

  return NextResponse.next();
}

export const config = {
  matcher: ['/((?!api/auth/logout).*)']
};
