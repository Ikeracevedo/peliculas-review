import { PrismaClient } from '../generated/prisma/client';
import { PrismaBetterSqlite3 } from '@prisma/adapter-better-sqlite3';

const adapter = new PrismaBetterSqlite3({
  url: process.env.DATABASE_URL ?? 'file:./dev.db',
});
const prisma = new PrismaClient({ adapter });

async function main() {
  // upsert por un campo unico real (correo) => idempotente de verdad
  const ana = await prisma.usuario.upsert({
    where: { correo: 'ana@ejemplo.com' },
    update: {},
    create: { correo: 'ana@ejemplo.com', nombre: 'Ana', password: 'temporal' },
  });

  // Pelicula no tiene un campo unico ademas del id, y el id autoincrement
  // no es estable entre corridas. Para un seed de desarrollo, basta con
  // limpiar la fila de prueba anterior antes de crear una nueva.
  await prisma.pelicula.deleteMany({ where: { nombre: 'Interstellar' } });

  const interstellar = await prisma.pelicula.create({
    data: {
      nombre: 'Interstellar',
      imagen: 'https://example.com/interstellar.jpg',
      estreno: 2014,
    },
  });

  console.log({ usuario: ana, pelicula: interstellar });
}

main()
  .catch((e) => {
    console.error(e);
    process.exit(1);
  })
  .finally(() => prisma.$disconnect());
