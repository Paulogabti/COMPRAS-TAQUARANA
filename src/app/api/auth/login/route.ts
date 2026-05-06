import { NextResponse } from 'next/server';
import bcrypt from 'bcryptjs';
import { prisma } from '@/lib/prisma';
import { setSessionCookie, signSession } from '@/lib/auth';

export async function POST(req: Request) {
  try {
    const form = await req.formData();
    const email = String(form.get('email') ?? '').toLowerCase().trim();
    const password = String(form.get('password') ?? '');

    if (!email || !password) {
      return NextResponse.redirect(new URL('/login?error=missing', req.url));
    }

    const user = await prisma.user.findUnique({ where: { email } });
    if (!user) {
      return NextResponse.redirect(new URL('/login?error=invalid', req.url));
    }

    const ok = await bcrypt.compare(password, user.passwordHash);
    if (!ok) {
      return NextResponse.redirect(new URL('/login?error=invalid', req.url));
    }

    const token = await signSession({ 
      sub: user.id, 
      role: user.role, 
      email: user.email, 
      name: user.name 
    });
    
    setSessionCookie(token);

    return NextResponse.redirect(new URL('/', req.url));
  } catch (error) {
    console.error('Login error:', error);
    return NextResponse.redirect(new URL('/login?error=server', req.url));
  }
}
