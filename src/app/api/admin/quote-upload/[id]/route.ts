import { NextResponse } from 'next/server';
import { getSession } from '@/lib/auth';
import { prisma } from '@/lib/prisma';
import { saveFile } from '@/lib/storage';

export async function POST(req: Request, { params }: { params: { id: string } }) {
  const session = await getSession();
  if (!session || session.role !== 'ADMIN') return new NextResponse('forbidden', { status: 403 });

  const form = await req.formData();
  const file = form.get('quoteFile');
  if (!(file instanceof File)) return NextResponse.redirect(new URL('/', req.url));

  const saved = await saveFile(file);

  const current = await prisma.request.findUnique({ where: { id: params.id }, select: { status: true } });
  if (!current) return new NextResponse('not found', { status: 404 });

  await prisma.request.update({
    where: { id: params.id },
    data: {
      quoteFile: saved.storageKey,
      quoteFileName: saved.originalName,
      status: 'FINALIZADO'
    }
  });

  await prisma.requestEvent.create({
    data: {
      requestId: params.id,
      fromStatus: current.status,
      toStatus: 'FINALIZADO',
      note: 'Arquivo de retorno anexado pelo ADMIN',
      actorId: session.sub
    }
  });

  return NextResponse.redirect(new URL('/', req.url));
}
