import { NextResponse } from 'next/server';
import { getSession } from '@/lib/auth';
import { prisma } from '@/lib/prisma';

export async function POST(req: Request, { params }: { params: { id: string } }) {
  const session = await getSession();
  if (!session || session.role !== 'ADMIN') return new NextResponse('forbidden', { status: 403 });

  const form = await req.formData();
  const status = String(form.get('status') ?? 'ABERTO') as 'ABERTO' | 'EM_COTACAO' | 'FINALIZADO' | 'CANCELADO';
  const note = String(form.get('note') ?? '').trim();

  const current = await prisma.request.findUnique({ where: { id: params.id }, select: { status: true } });
  if (!current) return new NextResponse('not found', { status: 404 });

  await prisma.request.update({ where: { id: params.id }, data: { status } });

  await prisma.requestEvent.create({
    data: {
      requestId: params.id,
      fromStatus: current.status,
      toStatus: status,
      note: note || null,
      actorId: session.sub
    }
  });

  return NextResponse.redirect(new URL('/', req.url));
}
