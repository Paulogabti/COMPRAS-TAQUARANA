import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';
import jwt from 'jsonwebtoken';

const publicRoutes = ['/login'];

export function middleware(req: NextRequest) {
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
      jwt.verify(token, process.env.JWT_SECRET!);
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
