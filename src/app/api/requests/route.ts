import { NextResponse } from 'next/server';
import { getSession } from '@/lib/auth';
import { saveFile } from '@/lib/storage';
import { prisma } from '@/lib/prisma';
import { z } from 'zod';

const schema = z.object({
  objeto: z.string().min(3),
  tipo: z.enum(['ADITIVO_PRAZO', 'CONTRATACAO_ARP', 'DISPENSA', 'LICITACAO', 'ADITIVO_ACRESCIMO']),
  dueDate: z.string().optional(),
  urgente: z.string().optional()
});

export async function POST(req: Request) {
  const session = await getSession();
  if (!session) return NextResponse.redirect(new URL('/login', req.url));

  const form = await req.formData();
  const parsed = schema.safeParse({
    objeto: String(form.get('objeto') ?? ''),
    tipo: String(form.get('tipo') ?? ''),
    dueDate: String(form.get('dueDate') ?? ''),
    urgente: String(form.get('urgente') ?? '')
  });

  const attachment = form.get('attachment');
  if (!parsed.success || !(attachment instanceof File)) {
    return NextResponse.redirect(new URL('/', req.url));
  }

  const ext = attachment.name.toLowerCase();
  if (!ext.endsWith('.pdf') && !ext.endsWith('.xlsx')) {
    return NextResponse.redirect(new URL('/', req.url));
  }

  const saved = await saveFile(attachment);

  const created = await prisma.request.create({
    data: {
      objeto: parsed.data.objeto,
      tipo: parsed.data.tipo,
      dueDate: parsed.data.dueDate ? new Date(parsed.data.dueDate) : null,
      urgente: !!parsed.data.urgente,
      attachment: saved.storageKey,
      attachmentName: saved.originalName,
      createdById: session.sub
    }
  });

  await prisma.requestEvent.create({
    data: {
      requestId: created.id,
      fromStatus: null,
      toStatus: 'ABERTO',
      note: 'Demanda criada no sistema',
      actorId: session.sub
    }
  });

  return NextResponse.redirect(new URL('/', req.url));
}
