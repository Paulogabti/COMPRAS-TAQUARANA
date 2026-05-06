# 🏛️ Compras Taquarana - Sistema de Gestão de Cotações

Este é um sistema profissional desenvolvido para gerenciar o fluxo de demandas de compras da Prefeitura de Taquarana. Ele permite que usuários enviem solicitações e que administradores gerenciem as cotações, gerem planilhas automáticas e anexem retornos.

---

## ✨ Funcionalidades Premium

- **Interface Moderna:** Design limpo, intuitivo e responsivo.
- **Gestão de Demandas:** Fluxo completo de ABERTO → EM COTAÇÃO → FINALIZADO.
- **Automação de Planilhas:** Gera automaticamente o `Modelo.xlsx` a partir de arquivos enviados.
- **Segurança Avançada:** Autenticação protegida com JWT e criptografia de senhas.
- **Relatórios:** Exportação de dados em CSV para análise administrativa.

---

## 🚀 Guia de Instalação (Para Leigos)

Este guia foi feito para que qualquer pessoa consiga colocar o sistema no ar usando a **Vercel**, que é gratuita e muito fácil de configurar.

### 1. Preparação
Você vai precisar de:
- Uma conta no [GitHub](https://github.com).
- Uma conta na [Vercel](https://vercel.com) (conectada ao seu GitHub).

### 2. Subindo para o GitHub
Se você já tem este código no seu GitHub, pule para o próximo passo. Caso contrário:
1. Crie um novo repositório no seu GitHub.
2. Envie os arquivos deste projeto para lá.

### 3. Deploy na Vercel (Passo a Passo)
1. No painel da Vercel, clique em **"Add New"** > **"Project"**.
2. Selecione o repositório `COMPRAS-TAQUARANA` e clique em **"Import"**.
3. Em **"Environment Variables"** (Variáveis de Ambiente), você precisará adicionar as seguintes chaves (copie e cole os nomes exatamente como estão):

| Nome da Variável | O que colocar? | Exemplo |
| :--- | :--- | :--- |
| `JWT_SECRET` | Uma frase longa e aleatória | `minha-frase-secreta-muito-segura-123` |
| `ADMIN_EMAIL` | O e-mail que você usará para entrar | `admin@taquarana.al.gov.br` |
| `ADMIN_PASSWORD` | A senha inicial do administrador | `admin123` |

4. **Banco de Dados:**
   - Após clicar em "Deploy", vá na aba **"Storage"** no painel do projeto na Vercel.
   - Escolha **"Postgres"** e clique em **"Create"**.
   - Depois de criado, clique em **"Connect"** para ligar o banco ao seu projeto. Isso criará a variável `DATABASE_URL` automaticamente.

5. **Armazenamento de Arquivos:**
   - Na mesma aba **"Storage"**, escolha **"Blob"** e clique em **"Create"**.
   - Clique em **"Connect"**. Isso permitirá que o sistema salve os PDFs e Planilhas que você enviar.

### 4. Finalização
1. Vá na aba **"Deployments"**, clique nos três pontinhos do seu deploy e selecione **"Redeploy"** para garantir que ele pegue todas as variáveis novas.
2. Assim que terminar, clique no link gerado pela Vercel e o sistema estará online!
3. Acesse com o e-mail e senha que você configurou no passo 3.

---

## 🛠️ Tecnologias Utilizadas
- **Next.js 14:** Framework web moderno.
- **Prisma & PostgreSQL:** Banco de dados robusto e confiável.
- **Vercel Blob:** Armazenamento seguro de arquivos na nuvem.
- **ExcelJS:** Manipulação inteligente de planilhas.

---

## 📝 Notas de Manutenção
Para atualizar o banco de dados localmente ou em produção, utilize o comando:
```bash
npx prisma db push
```
Para criar o usuário administrador inicial:
```bash
npm run seed
```

Desenvolvido com foco em eficiência e transparência para a gestão pública.
