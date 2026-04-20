const required = ['JWT_SECRET', 'DATABASE_URL'] as const;

export function validateEnv() {
  for (const key of required) {
    if (!process.env[key]) {
      throw new Error(`Variável obrigatória ausente: ${key}`);
    }
  }
}

export const isBlobEnabled = Boolean(process.env.BLOB_READ_WRITE_TOKEN);
