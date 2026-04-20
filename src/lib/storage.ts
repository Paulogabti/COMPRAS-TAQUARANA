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
    await put(key, buffer, {
      access: 'private',
      addRandomSuffix: false,
      token: process.env.BLOB_READ_WRITE_TOKEN
    });
    return { storageKey: key, originalName: file.name };
  }

  await mkdir(uploadDir, { recursive: true });
  await writeFile(path.join(uploadDir, key), buffer);
  return { storageKey: key, originalName: file.name };
}

export async function readFileFromStorage(storageKey: string): Promise<Buffer> {
  if (isBlobEnabled) {
    const endpoint = process.env.BLOB_BASE_URL;
    if (!endpoint) {
      throw new Error('BLOB_BASE_URL não configurada para leitura em produção.');
    }
    const url = `${endpoint.replace(/\/$/, '')}/${storageKey}`;
    const response = await fetch(url, {
      headers: {
        Authorization: `Bearer ${process.env.BLOB_READ_WRITE_TOKEN}`
      },
      cache: 'no-store'
    });
    if (!response.ok) throw new Error('Arquivo não encontrado no Blob.');
    return Buffer.from(await response.arrayBuffer());
  }

  return readFile(path.join(uploadDir, storageKey));
}

export function normalizeText250(text: string) {
  const normalized = text.replace(/\s+/g, ' ').trim();
  return normalized.slice(0, 250);
}
