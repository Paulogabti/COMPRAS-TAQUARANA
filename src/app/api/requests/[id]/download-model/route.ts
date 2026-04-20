import { NextResponse } from 'next/server';
import { getSession } from '@/lib/auth';
import { prisma } from '@/lib/prisma';
import { extractItemsFromSourceXlsx, generateModeloXlsx } from '@/lib/excel';
import { readFileFromStorage } from '@/lib/storage';

export async function GET(req: Request, { params }: { params: { id: string } }) {
  const session = await getSession();
  if (!session || session.role !== 'ADMIN') return new NextResponse('forbidden', { status: 403 });

  const demand = await prisma.request.findUnique({ where: { id: params.id } });
  if (!demand) return new NextResponse('not found', { status: 404 });

  if (!demand.attachmentName.toLowerCase().endsWith('.xlsx')) {
    return new NextResponse('Para gerar automático, anexe um .xlsx com os itens.', { status: 400 });
  }

  const source = await readFileFromStorage(demand.attachment);
  const items = await extractItemsFromSourceXlsx(source);
  const model = await generateModeloXlsx(items);

  return new NextResponse(model, {
    headers: {
      'Content-Type': 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
      'Content-Disposition': `attachment; filename="modelo-${params.id}.xlsx"`
    }
  });
}
