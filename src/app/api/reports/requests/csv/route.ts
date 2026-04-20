import { NextResponse } from 'next/server';
import { getSession } from '@/lib/auth';
import { prisma } from '@/lib/prisma';

export async function GET() {
  const session = await getSession();
  if (!session || session.role !== 'ADMIN') return new NextResponse('forbidden', { status: 403 });

  const requests = await prisma.request.findMany({
    include: { createdBy: { select: { name: true, email: true } } },
    orderBy: { createdAt: 'desc' }
  });

  const head = ['ID', 'OBJETO', 'TIPO', 'STATUS', 'URGENTE', 'VENCIMENTO', 'CRIADO EM', 'CRIADO POR', 'EMAIL'];
  const rows = requests.map((r) => [
    r.id,
    `"${r.objeto.replaceAll('"', '""')}"`,
    r.tipo,
    r.status,
    r.urgente ? 'SIM' : 'NÃO',
    r.dueDate ? r.dueDate.toISOString().slice(0, 10) : '',
    r.createdAt.toISOString(),
    r.createdBy.name,
    r.createdBy.email
  ].join(','));

  const csv = [head.join(','), ...rows].join('\n');

  return new NextResponse(csv, {
    headers: {
      'Content-Type': 'text/csv; charset=utf-8',
      'Content-Disposition': 'attachment; filename="relatorio-cotacoes.csv"'
    }
  });
}
