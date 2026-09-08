import * as bcrypt from 'bcrypt';
import { PrismaClient } from '../generated/prisma/client';
import { PrismaBetterSqlite3 } from '@prisma/adapter-better-sqlite3';

const adapter = new PrismaBetterSqlite3({
  url: process.env.DATABASE_URL ?? 'file:./dev.db',
});
const prisma = new PrismaClient({ adapter });

// Contraseña de prueba para AMBOS usuarios sembrados. Solo para desarrollo.
const PASSWORD_DE_PRUEBA = 'Password123!';

// Catálogo de prueba: suficientes películas para probar paginación,
// búsqueda y todo el CRUD desde el front sin depender de datos reales.
const PELICULAS = [
  { nombre: 'Interstellar', estreno: 2014 },
  { nombre: 'Inception', estreno: 2010 },
  { nombre: 'The Matrix', estreno: 1999 },
  { nombre: 'Pulp Fiction', estreno: 1994 },
  { nombre: 'The Godfather', estreno: 1972 },
  { nombre: 'Forrest Gump', estreno: 1994 },
  { nombre: 'The Dark Knight', estreno: 2008 },
  { nombre: 'Fight Club', estreno: 1999 },
  { nombre: 'Parasite', estreno: 2019 },
  { nombre: 'Whiplash', estreno: 2014 },
  { nombre: 'Coco', estreno: 2017 },
  { nombre: 'Spirited Away', estreno: 2001 },
  { nombre: 'The Shawshank Redemption', estreno: 1994 },
  { nombre: 'Gladiator', estreno: 2000 },
  { nombre: 'Titanic', estreno: 1997 },
  { nombre: 'Avatar', estreno: 2009 },
  { nombre: 'Joker', estreno: 2019 },
  { nombre: 'La La Land', estreno: 2016 },
  { nombre: 'Get Out', estreno: 2017 },
  { nombre: 'Everything Everywhere All at Once', estreno: 2022 },
];

async function main() {
  const passwordHasheada = await bcrypt.hash(PASSWORD_DE_PRUEBA, 10);

  const usuario = await prisma.usuario.upsert({
    where: { correo: 'ana@ejemplo.com' },
    update: {},
    create: {
      correo: 'maria@ejemplo.com',
      nombre: 'Maria',
      password: passwordHasheada,
      // rol se omite => usa el default del schema: USUARIO
    },
  });

  const admin = await prisma.usuario.upsert({
    where: { correo: 'admin@ejemplo.com' },
    update: {},
    create: {
      correo: 'admin@ejemplo.com',
      nombre: 'Admin',
      password: passwordHasheada,
      rol: 'ADMIN',
    },
  });

  // Reinicia el catalogo para que el seed sea reproducible en cualquier
  // maquina. onDelete: Cascade en Review limpia solas las resenas huerfanas.
  await prisma.pelicula.deleteMany({});

  await prisma.pelicula.createMany({
    data: PELICULAS.map((p) => ({
      nombre: p.nombre,
      estreno: p.estreno,
      // Imagenes de placeholder (no son posters reales, evita temas de
      // licencias). El seed es distinto y estable por titulo.
      imagen: `https://picsum.photos/seed/${encodeURIComponent(p.nombre)}/400/600`,
    })),
  });

  console.log('Usuarios de prueba:');
  console.log(`  ${usuario.correo} (rol: ${usuario.rol})`);
  console.log(`  ${admin.correo} (rol: ${admin.rol})`);
  console.log(`  Password para ambos: "${PASSWORD_DE_PRUEBA}"`);
  console.log(`${PELICULAS.length} peliculas creadas.`);
}

main()
  .catch((e) => {
    console.error(e);
    process.exit(1);
  })
  .finally(() => prisma.$disconnect());