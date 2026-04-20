# Sistema de Gestão de Cotações - Compras Taquarana

Aplicação para fluxo de demandas da equipe de Compras com gestão completa de cotações pelo ADMIN.

## O que já está pronto para produção

- Login com perfis `ADMIN` e `USER`.
- Usuário comum abre demanda com:
  - Objeto
  - Tipo de cotação
  - Data de vencimento opcional
  - Urgente opcional
  - Anexo `.pdf` ou `.xlsx`
- Status inicial: `ABERTO`.
- ADMIN pode alterar status para: `EM_COTACAO`, `FINALIZADO`, `CANCELADO`.
- Histórico de status (trilha de auditoria com data/hora, ator e observação).
- Upload de arquivo de retorno da cotação pelo ADMIN (`.zip/.pdf/.xlsx`).
- Download seguro de anexos originais e arquivo de retorno.
- Geração automática do `Modelo.xlsx` quando a origem for `.xlsx`.
- Regra de negócio aplicada: **Nome limitado a 250 caracteres** na planilha modelo.
- Relatório de demandas em CSV para ADMIN.
- Pronto para Vercel com:
  - Banco Postgres (`DATABASE_URL`)
  - Storage persistente via Vercel Blob (`BLOB_READ_WRITE_TOKEN` + `BLOB_BASE_URL`)

## Stack

- Next.js 14 (App Router)
- TypeScript
- Prisma ORM
- PostgreSQL
- Vercel Blob (produção)
- ExcelJS (leitura/geração de `.xlsx`)

---

## 1) Configuração local (desenvolvimento)

### Pré-requisitos
- Node.js 20+
- NPM
- PostgreSQL disponível (local ou remoto)

### Passo a passo
1. Copie variáveis:
   ```bash
   cp .env.example .env
   ```
2. Ajuste `.env` com seu Postgres e segredo JWT.
3. Instale dependências:
   ```bash
   npm install
   ```
4. Gere migration inicial e Prisma Client:
   ```bash
   npx prisma migrate dev --name init
   ```
5. Crie o ADMIN:
   ```bash
   npm run seed
   ```
6. Rode a aplicação:
   ```bash
   npm run dev
   ```

---

## 2) Deploy completo na Vercel (produção)

## 2.1 Subir código no GitHub
1. Garanta que este projeto está no seu repositório do GitHub.
2. Faça push da branch principal.

## 2.2 Criar projeto na Vercel
1. Acesse Vercel > **Add New Project**.
2. Selecione o repositório do GitHub.
3. Framework detectado: **Next.js**.

## 2.3 Banco de dados (Vercel Postgres)
1. No painel da Vercel, abra **Storage**.
2. Clique em **Create Database** > **Postgres**.
3. Após criar, conecte ao projeto.
4. A Vercel preencherá automaticamente `POSTGRES_PRISMA_URL` e outras variáveis.
5. Defina `DATABASE_URL` usando a URL Prisma (normalmente `POSTGRES_PRISMA_URL`).

## 2.4 Armazenamento de arquivos (Vercel Blob)
1. No painel da Vercel, abra **Storage**.
2. Clique em **Create** > **Blob**.
3. Conecte ao projeto.
4. Copie e configure no projeto:
   - `BLOB_READ_WRITE_TOKEN`
   - `BLOB_BASE_URL` (base pública do bucket)

## 2.5 Variáveis de ambiente obrigatórias
No projeto Vercel > **Settings** > **Environment Variables**, configure:

- `DATABASE_URL`
- `JWT_SECRET` (use um valor forte, 32+ caracteres)
- `ADMIN_EMAIL`
- `ADMIN_PASSWORD`
- `BLOB_READ_WRITE_TOKEN`
- `BLOB_BASE_URL`

> Recomendo criar valores para `Production`, `Preview` e `Development`.

## 2.6 Build command e deploy
- Build command padrão do projeto já está preparado:
  - `npm run build`
- Ele executa `prisma generate` + `next build`.

## 2.7 Rodar migrations em produção
Após o primeiro deploy, execute migration no banco de produção:

Opção A (recomendada): pipeline CI/CD rodando
```bash
npx prisma migrate deploy
```

Opção B: localmente, apontando para o mesmo `DATABASE_URL` de produção.

## 2.8 Criar usuário ADMIN no ambiente de produção
Com variáveis `ADMIN_EMAIL` e `ADMIN_PASSWORD` já definidas, rode:
```bash
npm run seed
```

Você pode executar isso em ambiente controlado de CI/CD ou terminal com `DATABASE_URL` de produção.

---

## 3) Fluxo funcional em produção

1. Equipe (USER) abre demanda e envia PDF/XLSX.
2. Demanda entra como `ABERTO`.
3. ADMIN muda status para `EM_COTACAO` e pode registrar observação.
4. ADMIN baixa anexo e gera `Modelo.xlsx` (quando origem for XLSX).
5. ADMIN anexa retorno final (zip/pdf/xlsx); status passa para `FINALIZADO`.
6. USER baixa o arquivo final.
7. ADMIN baixa relatório CSV consolidado.

---

## 4) Boas práticas para estabilizar ainda mais

- Habilitar monitoramento (Vercel Observability / Sentry).
- Criar rotina de backup do banco Postgres.
- Adicionar renovação/expiração de sessão com refresh controlado.
- Implantar fila para processamento de arquivos muito grandes.
- Evoluir parser de PDF (OCR) se quiser geração automática também para PDF.

---

## Observação técnica sobre geração de `Modelo.xlsx`

A extração automática é confiável quando o arquivo de entrada é `.xlsx`.
Para PDF, a estrutura varia muito entre documentos; por isso, no desenho atual, a geração automática exige `.xlsx` como fonte.
