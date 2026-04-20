import ExcelJS from 'exceljs';
import { normalizeText250 } from './storage';

export type ModeloItem = {
  nome: string;
  descricao: string;
  quantidade: string | number;
  unidade: string;
};

export async function extractItemsFromSourceXlsx(buffer: Buffer): Promise<ModeloItem[]> {
  const workbook = new ExcelJS.Workbook();
  const arrayBuffer = buffer.buffer.slice(buffer.byteOffset, buffer.byteOffset + buffer.byteLength) as ArrayBuffer;
  await workbook.xlsx.load(arrayBuffer);

  const ws = workbook.worksheets[0];
  if (!ws) return [];

  const rows: ModeloItem[] = [];
  ws.eachRow((row, rowNumber) => {
    if (rowNumber === 1) return;
    const nomeRaw = String(row.getCell(2).text ?? '').trim();
    const descRaw = String(row.getCell(3).text ?? '').trim();
    const qtdRaw = row.getCell(5).text || row.getCell(4).text || '';
    const unRaw = String(row.getCell(6).text || row.getCell(7).text || '').trim();
    if (!nomeRaw && !descRaw) return;

    rows.push({
      nome: normalizeText250(nomeRaw || descRaw),
      descricao: descRaw || nomeRaw,
      quantidade: qtdRaw || '1',
      unidade: unRaw || 'UN'
    });
  });

  return rows;
}

export async function generateModeloXlsx(items: ModeloItem[]) {
  const workbook = new ExcelJS.Workbook();
  const ws = workbook.addWorksheet('Modelo');

  ws.columns = [
    { header: 'Nome (Até 250 caracteres)', key: 'nome', width: 45 },
    { header: 'Descrição (Texto Livre)', key: 'descricao', width: 70 },
    { header: 'Quantidade', key: 'quantidade', width: 15 },
    { header: 'Unidade De Medida', key: 'unidade', width: 20 }
  ];

  for (const item of items) {
    ws.addRow({
      nome: normalizeText250(item.nome),
      descricao: item.descricao,
      quantidade: item.quantidade,
      unidade: item.unidade
    });
  }

  ws.getRow(1).font = { bold: true };
  return Buffer.from(await workbook.xlsx.writeBuffer());
}
