import { prisma } from '@/lib/prisma';
import { getSession } from '@/lib/auth';
import { redirect } from 'next/navigation';

const tipoOptions = ['ADITIVO_PRAZO', 'CONTRATACAO_ARP', 'DISPENSA', 'LICITACAO', 'ADITIVO_ACRESCIMO'] as const;

export default async function HomePage() {
  const session = await getSession();
  if (!session) redirect('/login');

  const requests = await prisma.request.findMany({
    where: session.role === 'ADMIN' ? {} : { createdById: session.sub },
    include: {
      createdBy: { select: { name: true, email: true } },
      events: { orderBy: { createdAt: 'desc' }, take: 3, include: { actor: { select: { name: true } } } }
    },
    orderBy: { createdAt: 'desc' }
  });

  const total = requests.length;
  const concluidas = requests.filter((r) => r.status === 'FINALIZADO').length;

  return (
    <main>
      <div className="header">
        <div>
          <h1>Painel de Cotações</h1>
          <small className="muted">Olá, {session.name} ({session.role})</small>
        </div>
        <div style={{ display: 'flex', gap: 8 }}>
          {session.role === 'ADMIN' && <a href="/api/reports/requests/csv"><button className="secondary" type="button">Baixar relatório CSV</button></a>}
          <form action="/api/auth/logout" method="post">
            <button className="secondary">Sair</button>
          </form>
        </div>
      </div>

      <section className="grid grid-2" style={{ marginBottom: 16 }}>
        <article className="card"><strong>Total</strong><div>{total}</div></article>
        <article className="card"><strong>Finalizadas</strong><div>{concluidas}</div></article>
      </section>

      <section className="grid grid-2">
        <article className="card">
          <h3>Nova demanda</h3>
          <form className="grid" action="/api/requests" method="post" encType="multipart/form-data">
            <div><label>OBJETO</label><textarea required name="objeto" /></div>
            <div>
              <label>TIPO DE COTAÇÃO</label>
              <select name="tipo" required>
                {tipoOptions.map((t) => <option value={t} key={t}>{t.replaceAll('_', ' ')}</option>)}
              </select>
            </div>
            <div><label>DATA DO VENCIMENTO (OPCIONAL)</label><input name="dueDate" type="date" /></div>
            <div><label><input type="checkbox" name="urgente" /> URGENTE</label></div>
            <div><label>ANEXO (PDF ou XLSX)</label><input name="attachment" type="file" accept=".pdf,.xlsx" required /></div>
            <button type="submit">Cadastrar (Status ABERTO)</button>
          </form>
        </article>

        {session.role === 'ADMIN' && (
          <article className="card">
            <h3>Relatório rápido</h3>
            <p>Abertos: {requests.filter((r) => r.status === 'ABERTO').length}</p>
            <p>Em cotação: {requests.filter((r) => r.status === 'EM_COTACAO').length}</p>
            <p>Cancelados: {requests.filter((r) => r.status === 'CANCELADO').length}</p>
            <p>Finalizados: {concluidas}</p>
          </article>
        )}
      </section>

      <section className="card" style={{ marginTop: 16 }}>
        <h3>Demandas</h3>
        <table className="table">
          <thead><tr><th>Objeto</th><th>Tipo</th><th>Status</th><th>Criado por</th><th>Ações</th></tr></thead>
          <tbody>
            {requests.map((r) => (
              <tr key={r.id}>
                <td>{r.objeto}</td>
                <td>{r.tipo.replaceAll('_', ' ')}</td>
                <td><span className={`badge status-${r.status}`}>{r.status.replaceAll('_', ' ')}</span></td>
                <td>{r.createdBy.name}</td>
                <td>
                  <div style={{ display: 'grid', gap: 8 }}>
                    <a href={`/api/uploads/${r.id}`}>Baixar anexo</a>
                    {session.role === 'ADMIN' && <a href={`/api/requests/${r.id}/download-model`}>Baixar Modelo.xlsx</a>}
                    {r.quoteFile && <a href={`/api/uploads/${r.id}?kind=quote`}>Baixar retorno cotado</a>}
                    {session.role === 'ADMIN' && (
                      <>
                        <form action={`/api/requests/${r.id}/status`} method="post" style={{ display: 'grid', gap: 8 }}>
                          <select name="status" defaultValue={r.status}>
                            <option value="ABERTO">ABERTO</option>
                            <option value="EM_COTACAO">EM COTAÇÃO</option>
                            <option value="FINALIZADO">FINALIZADO</option>
                            <option value="CANCELADO">CANCELADO</option>
                          </select>
                          <input name="note" placeholder="Observação (opcional)" />
                          <button>Salvar status</button>
                        </form>
                        <form action={`/api/admin/quote-upload/${r.id}`} method="post" encType="multipart/form-data">
                          <input type="file" name="quoteFile" accept=".zip,.pdf,.xlsx" required />
                          <button>Enviar retorno</button>
                        </form>
                      </>
                    )}
                    <details>
                      <summary>Histórico recente</summary>
                      <ul>
                        {r.events.map((ev) => (
                          <li key={ev.id}>
                            {new Date(ev.createdAt).toLocaleString('pt-BR')} - {ev.actor.name}: {ev.fromStatus ? `${ev.fromStatus} → ` : ''}{ev.toStatus}
                            {ev.note ? ` (${ev.note})` : ''}
                          </li>
                        ))}
                      </ul>
                    </details>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>
    </main>
  );
}
