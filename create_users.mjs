import bcrypt from 'bcryptjs';
import { PrismaClient } from '@prisma/client';

const prisma = new PrismaClient();

async function main() {
  const users = [
    {
      name: 'Paulo Admin',
      email: 'paulo@taquarana.com',
      password: 'paulo12',
      role: 'ADMIN'
    },
    {
      name: 'Compras Usuário',
      email: 'compras@taquarana.com',
      password: 'compras12',
      role: 'USER'
    }
  ];

  for (const u of users) {
    const hash = await bcrypt.hash(u.password, 12);
    await prisma.user.upsert({
      where: { email: u.email.toLowerCase().trim() },
      update: { passwordHash: hash, role: u.role, name: u.name },
      create: {
        name: u.name,
        email: u.email.toLowerCase().trim(),
        passwordHash: hash,
        role: u.role
      }
    });
    console.log(`Usuário ${u.email} criado/atualizado com sucesso.`);
  }
}

main()
  .catch(e => console.error(e))
  .finally(async () => {
    await prisma.$disconnect();
  });
