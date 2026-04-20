import { NextResponse } from 'next/server';
import { getSession } from '@/lib/auth';
import { prisma } from '@/lib/prisma';
import { readFileFromStorage } from '@/lib/storage';

export async function GET(req: Request, { params }: { params: { id: string } }) {
  const session = await getSession();
  if (!session) return new NextResponse('forbidden', { status: 403 });

  const { searchParams } = new URL(req.url);
  const kind = searchParams.get('kind');

  const r = await prisma.request.findUnique({ where: { id: params.id } });
  if (!r) return new NextResponse('not found', { status: 404 });
  if (session.role !== 'ADMIN' && session.sub !== r.createdById) return new NextResponse('forbidden', { status: 403 });

  const stored = kind === 'quote' ? r.quoteFile : r.attachment;
  const filename = kind === 'quote' ? r.quoteFileName : r.attachmentName;

  if (!stored || !filename) return new NextResponse('not found', { status: 404 });

  const buffer = await readFileFromStorage(stored);
  const body = new Blob([new Uint8Array(buffer)]);

  return new NextResponse(body, {
    headers: {
      'Content-Type': 'application/octet-stream',
      'Content-Disposition': `attachment; filename="${filename}"`
    }
  });
}
