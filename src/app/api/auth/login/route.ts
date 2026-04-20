import { NextResponse } from 'next/server';
import bcrypt from 'bcryptjs';
import { prisma } from '@/lib/prisma';
import { setSessionCookie, signSession } from '@/lib/auth';

export async function POST(req: Request) {
  const form = await req.formData();
  const email = String(form.get('email') ?? '').toLowerCase().trim();
  const password = String(form.get('password') ?? '');

  const user = await prisma.user.findUnique({ where: { email } });
  if (!user) return NextResponse.redirect(new URL('/login', req.url));

  const ok = await bcrypt.compare(password, user.passwordHash);
  if (!ok) return NextResponse.redirect(new URL('/login', req.url));

  const token = signSession({ sub: user.id, role: user.role, email: user.email, name: user.name });
  setSessionCookie(token);

  return NextResponse.redirect(new URL('/', req.url));
}
