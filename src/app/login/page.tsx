'use client';

import { useSearchParams } from 'next/navigation';
import { Suspense } from 'react';

function LoginContent() {
  const searchParams = useSearchParams();
  const error = searchParams.get('error');

  const errorMessages: Record<string, string> = {
    missing: 'Por favor, preencha todos os campos.',
    invalid: 'E-mail ou senha incorretos.',
    server: 'Erro no servidor. Tente novamente mais tarde.',
  };

  return (
    <div className="login-container">
      <main className="login-card">
        <div className="card">
          <div style={{ textAlign: 'center', marginBottom: '2rem' }}>
            <h1 style={{ fontSize: '1.5rem', color: 'var(--primary)' }}>Compras Taquarana</h1>
            <p style={{ color: 'var(--text-muted)', fontSize: '0.875rem' }}>Sistema de Gestão de Cotações</p>
          </div>

          {error && errorMessages[error] && (
            <div className="error-message">
              {errorMessages[error]}
            </div>
          )}

          <form action="/api/auth/login" method="post" className="grid">
            <div className="form-group">
              <label htmlFor="email">E-mail</label>
              <input 
                id="email"
                name="email" 
                type="email" 
                placeholder="seu@email.com" 
                required 
              />
            </div>
            
            <div className="form-group">
              <label htmlFor="password">Senha</label>
              <input 
                id="password"
                name="password" 
                type="password" 
                placeholder="••••••••" 
                required 
              />
            </div>

            <button type="submit" style={{ marginTop: '1rem' }}>
              Entrar no Sistema
            </button>
          </form>
          
          <div style={{ marginTop: '1.5rem', textAlign: 'center' }}>
            <small style={{ color: 'var(--text-muted)' }}>
              &copy; 2024 Prefeitura de Taquarana
            </small>
          </div>
        </div>
      </main>
    </div>
  );
}

export default function LoginPage() {
  return (
    <Suspense fallback={<div>Carregando...</div>}>
      <LoginContent />
    </Suspense>
  );
}
