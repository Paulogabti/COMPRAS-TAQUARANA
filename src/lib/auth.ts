import { cookies } from 'next/headers';
import { SignJWT, jwtVerify } from 'jose';

export type SessionPayload = {
  sub: string;
  role: 'ADMIN' | 'USER';
  name: string;
  email: string;
};

const COOKIE_NAME = 'compras_session';
const SECRET = new TextEncoder().encode(process.env.JWT_SECRET || 'fallback-secret-para-build');

export async function signSession(payload: SessionPayload) {
  return await new SignJWT(payload)
    .setProtectedHeader({ alg: 'HS256' })
    .setIssuedAt()
    .setExpirationTime('7d')
    .sign(SECRET);
}

export async function verifySession(token: string): Promise<SessionPayload | null> {
  try {
    const { payload } = await jwtVerify(token, SECRET);
    return payload as unknown as SessionPayload;
  } catch {
    return null;
  }
}

export async function getSession() {
  const token = cookies().get(COOKIE_NAME)?.value;
  if (!token) return null;
  return await verifySession(token);
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
