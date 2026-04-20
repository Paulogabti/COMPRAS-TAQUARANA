import jwt from 'jsonwebtoken';
import { cookies } from 'next/headers';

export type SessionPayload = {
  sub: string;
  role: 'ADMIN' | 'USER';
  name: string;
  email: string;
};

const COOKIE_NAME = 'compras_session';

export function signSession(payload: SessionPayload) {
  const secret = process.env.JWT_SECRET;
  if (!secret) throw new Error('JWT_SECRET não configurado');
  return jwt.sign(payload, secret, { expiresIn: '7d' });
}

export function verifySession(token: string): SessionPayload | null {
  const secret = process.env.JWT_SECRET;
  if (!secret) return null;
  try {
    return jwt.verify(token, secret) as SessionPayload;
  } catch {
    return null;
  }
}

export async function getSession() {
  const token = cookies().get(COOKIE_NAME)?.value;
  if (!token) return null;
  return verifySession(token);
}

export function setSessionCookie(token: string) {
  cookies().set(COOKIE_NAME, token, {
    httpOnly: true,
    secure: process.env.NODE_ENV === 'production',
    sameSite: 'lax',
    path: '/',
    maxAge: 60 * 60 * 24 * 7
  });
}

export function clearSessionCookie() {
  cookies().set(COOKIE_NAME, '', { path: '/', expires: new Date(0) });
}
