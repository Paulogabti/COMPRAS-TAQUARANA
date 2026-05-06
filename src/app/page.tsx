import { prisma } from '@/lib/prisma';
import { getSession } from '@/lib/auth';
import { redirect } from 'next/navigation';

const tipoOptions = [
  { value: 'ADITIVO_PRAZO', label: 'Aditivo de Prazo' },
  { value: 'CONTRATACAO_ARP', label: 'Contratação ARP' },
  { value: 'DISPENSA', label: 'Dispensa' },
  { value: 'LICITACAO', label: 'Licitação' },
  { value: 'ADITIVO_ACRESCIMO', label: 'Aditivo de Acréscimo' }
] as const;

export default async function HomePage() {
  const session = await getSession();
  if (!session) redirect('/login');

  const requests = await prisma.request.findMany({
    where: session.role === 'ADMIN' ? {} : { createdById: session.sub },
    include: {
      createdBy: { select: { name: true, email: true } },
      events: { orderBy: { createdAt: 'desc' }, take: 5, include: { actor: { select: { name: true } } } }
    },
    orderBy: { createdAt: 'desc' }
  });

  const total = requests.length;
  const concluidas = requests.filter((r) => r.status === 'FINALIZADO').length;
  const abertas = requests.filter((r) => r.status === 'ABERTO').length;
  const emCotacao = requests.filter((r) => r.status === 'EM_COTACAO').length;

  return (
    <main>
      <header className="header">
        <div>
          <h1>Painel de Cotações</h1>
          <small>Bem-vindo, <strong>{session.name}</strong> • Perfil: {session.role}</small>
        </div>
        <div style={{ display: 'flex', gap: '12px' }}>
          {session.role === 'ADMIN' && (
            <a href="/api/reports/requests/csv">
              <button className="secondary" type="button">
                <span>📊</span> Exportar CSV
              </button>
            </a>
          )}
          <form action="/api/auth/logout" method="post">
            <button className="secondary">
              <span>🚪</span> Sair
            </button>
          </form>
        </div>
      </header>

      <section className="stats-grid">
        <div className="stat-card">
          <strong>Total de Demandas</strong>
          <div className="value">{total}</div>
        </div>
        <div className="stat-card">
          <strong>Em Aberto</strong>
          <div className="value" style={{ color: 'var(--success)' }}>{abertas}</div>
        </div>
        <div className="stat-card">
          <strong>Em Cotação</strong>
          <div className="value" style={{ color: 'var(--warning)' }}>{emCotacao}</div>
        </div>
        <div className="stat-card">
          <strong>Finalizadas</strong>
          <div className="value" style={{ color: 'var(--primary)' }}>{concluidas}</div>
        </div>
      </section>

      <div className="grid grid-2" style={{ alignItems: 'start' }}>
        <section className="card">
          <h3 style={{ marginBottom: '1.5rem', display: 'flex', alignItems: 'center', gap: '8px' }}>
            <span>📝</span> Nova Demanda
          </h3>
          <form action="/api/requests" method="post" encType="multipart/form-data" className="grid">
            <div className="form-group">
              <label>Objeto da Cotação</label>
              <textarea 
                required 
                name="objeto" 
                placeholder="Descreva o que precisa ser cotado..." 
                rows={3}
              />
            </div>
            
            <div className="form-group">
              <label>Tipo de Cotação</label>
              <select name="tipo" required>
                {tipoOptions.map((t) => <option value={t.value} key={t.value}>{t.label}</option>)}
              </select>
            </div>

            <div className="grid" style={{ gridTemplateColumns: '1fr 1fr' }}>
              <div className="form-group">
                <label>Data de Vencimento</label>
                <input name="dueDate" type="date" />
              </div>
              <div className="form-group" style={{ display: 'flex', alignItems: 'flex-end', paddingBottom: '0.75rem' }}>
                <label className="urgente-checkbox">
                  <input type="checkbox" name="urgente" /> 
                  <span style={{ color: 'var(--danger)' }}>Marcar como Urgente</span>
                </label>
              </div>
            </div>

            <div className="form-group">
              <label>Anexo (PDF ou XLSX)</label>
              <input 
                name="attachment" 
                type="file" 
                accept=".pdf,.xlsx" 
                required 
                style={{ padding: '0.5rem' }}
              />
            </div>

            <button type="submit" style={{ width: '100%' }}>
              Criar Demanda
            </button>
          </form>
        </section>

        <section className="card">
          <h3 style={{ marginBottom: '1.5rem' }}>Lista de Demandas</h3>
          <div className="table-container">
            <table className="table">
              <thead>
                <tr>
                  <th>Objeto</th>
                  <th>Status</th>
                  <th>Ações</th>
                </tr>
              </thead>
              <tbody>
                {requests.length === 0 ? (
                  <tr>
                    <td colSpan={3} style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-muted)' }}>
                      Nenhuma demanda encontrada.
                    </td>
                  </tr>
                ) : (
                  requests.map((r) => (
                    <tr key={r.id}>
                      <td>
                        <div style={{ fontWeight: '600' }}>{r.objeto.substring(0, 50)}{r.objeto.length > 50 ? '...' : ''}</div>
                        <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>
                          {r.tipo.replaceAll('_', ' ')} • {new Date(r.createdAt).toLocaleDateString('pt-BR')}
                        </div>
                        {r.urgente && <span style={{ fontSize: '0.7rem', color: 'var(--danger)', fontWeight: 'bold' }}>⚡ URGENTE</span>}
                      </td>
                      <td>
                        <span className={`badge status-${r.status}`}>
                          {r.status.replaceAll('_', ' ')}
                        </span>
                      </td>
                      <td>
                        <div className="action-links">
                          <a href={`/api/uploads/${r.id}`}>Anexo</a>
                          {session.role === 'ADMIN' && (
                            <a href={`/api/requests/${r.id}/download-model`} style={{ color: 'var(--success)' }}>Modelo</a>
                          )}
                          {r.quoteFile && (
                            <a href={`/api/uploads/${r.id}?kind=quote`} style={{ fontWeight: 'bold' }}>Retorno</a>
                          )}
                        </div>
                        
                        {session.role === 'ADMIN' && (
                          <details style={{ marginTop: '0.5rem' }}>
                            <summary>Gerenciar</summary>
                            <div style={{ padding: '0.5rem', background: '#f8fafc', borderRadius: '8px', marginTop: '0.5rem' }}>
                              <form action={`/api/requests/${r.id}/status`} method="post" className="grid" style={{ gap: '8px' }}>
                                <select name="status" defaultValue={r.status} style={{ padding: '4px', fontSize: '0.8rem' }}>
                                  <option value="ABERTO">ABERTO</option>
                                  <option value="EM_COTACAO">EM COTAÇÃO</option>
                                  <option value="FINALIZADO">FINALIZADO</option>
                                  <option value="CANCELADO">CANCELADO</option>
                                </select>
                                <input name="note" placeholder="Nota..." style={{ padding: '4px', fontSize: '0.8rem' }} />
                                <button style={{ padding: '4px', fontSize: '0.8rem' }}>Atualizar</button>
                              </form>
                              
                              <hr style={{ margin: '8px 0', border: 'none', borderTop: '1px solid var(--border)' }} />
                              
                              <form action={`/api/admin/quote-upload/${r.id}`} method="post" encType="multipart/form-data" className="grid" style={{ gap: '8px' }}>
                                <label style={{ fontSize: '0.7rem', marginBottom: 0 }}>Subir Retorno:</label>
                                <input type="file" name="quoteFile" accept=".zip,.pdf,.xlsx" required style={{ padding: '2px', fontSize: '0.7rem' }} />
                                <button className="secondary" style={{ padding: '4px', fontSize: '0.8rem' }}>Enviar</button>
                              </form>
                            </div>
                          </details>
                        )}

                        <details>
                          <summary>Histórico</summary>
                          <ul className="history-list">
                            {r.events.map((ev) => (
                              <li key={ev.id} className="history-item">
                                <strong>{new Date(ev.createdAt).toLocaleDateString('pt-BR')}</strong> - {ev.actor.name}: 
                                {ev.fromStatus ? ` ${ev.fromStatus} → ` : ' '} 
                                <span style={{ fontWeight: '600' }}>{ev.toStatus}</span>
                                {ev.note && <div style={{ fontStyle: 'italic', marginTop: '2px' }}>"{ev.note}"</div>}
                              </li>
                            ))}
                          </ul>
                        </details>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </section>
      </div>
    </main>
  );
}
