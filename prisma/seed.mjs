import bcrypt from 'bcryptjs';
import { PrismaClient } from '@prisma/client';

const prisma = new PrismaClient();

async function main() {
  const adminEmail = process.env.ADMIN_EMAIL;
  const adminPassword = process.env.ADMIN_PASSWORD;

  if (!adminEmail || !adminPassword) {
    throw new Error('Defina ADMIN_EMAIL e ADMIN_PASSWORD antes de rodar o seed.');
  }

  const hash = await bcrypt.hash(adminPassword, 12);

  await prisma.user.upsert({
    where: { email: adminEmail.toLowerCase().trim() },
    update: { passwordHash: hash, role: 'ADMIN', name: 'Administrador' },
    create: {
      name: 'Administrador',
      email: adminEmail.toLowerCase().trim(),
      passwordHash: hash,
      role: 'ADMIN'
    }
  });
}

main().finally(async () => prisma.$disconnect());
