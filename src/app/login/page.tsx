export default function LoginPage() {
  return (
    <main style={{ maxWidth: 420, paddingTop: 80 }}>
      <form className="card grid" action="/api/auth/login" method="post">
        <h1>Compras Taquarana</h1>
        <small className="muted">Acesse com seu e-mail e senha.</small>
        <div>
          <label>E-mail</label>
          <input name="email" type="email" required />
        </div>
        <div>
          <label>Senha</label>
          <input name="password" type="password" required />
        </div>
        <button type="submit">Entrar</button>
      </form>
    </main>
  );
}
