import { mkdir, readFile, writeFile } from 'node:fs/promises';
import path from 'node:path';
import crypto from 'node:crypto';
import { put } from '@vercel/blob';
import { isBlobEnabled } from './env';

const uploadDir = path.join(process.cwd(), 'uploads');

export type StoredFile = {
  storageKey: string;
  originalName: string;
};

export async function saveFile(file: File): Promise<StoredFile> {
  const ext = path.extname(file.name) || '.bin';
  const key = `${crypto.randomUUID()}${ext}`;
  const buffer = Buffer.from(await file.arrayBuffer());

  if (isBlobEnabled) {
    const blob = await put(key, buffer, {
      access: 'public',
      addRandomSuffix: false,
      token: process.env.BLOB_READ_WRITE_TOKEN
    });
    return { storageKey: blob.url, originalName: file.name };
  }

  await mkdir(uploadDir, { recursive: true });
  await writeFile(path.join(uploadDir, key), buffer);
  return { storageKey: key, originalName: file.name };
}

export async function readFileFromStorage(storageKey: string): Promise<Buffer> {
  if (isBlobEnabled) {
    // Se a storageKey já for uma URL completa (como as novas que vamos salvar)
    const url = storageKey.startsWith('http') ? storageKey : `${(process.env.BLOB_BASE_URL || '').replace(/\/$/, '')}/${storageKey}`;
    
    if (!url.startsWith('http')) {
      throw new Error('URL do Blob inválida ou BLOB_BASE_URL não configurada.');
    }

    const response = await fetch(url, {
      cache: 'no-store'
    });
    if (!response.ok) throw new Error(`Arquivo não encontrado no Blob: ${response.statusText}`);
    return Buffer.from(await response.arrayBuffer());
  }

  return readFile(path.join(uploadDir, storageKey));
}

export function normalizeText250(text: string) {
  const normalized = text.replace(/\s+/g, ' ').trim();
  return normalized.slice(0, 250);
}
