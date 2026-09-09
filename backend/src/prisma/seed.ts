import "dotenv/config";
import * as bcrypt from "bcrypt";
import { PrismaClient } from "../generated/prisma/client";
import { PrismaLibSql } from "@prisma/adapter-libsql";

const adapter = new PrismaLibSql({ url: "file:./dev.db" });
const prisma = new PrismaClient({ adapter });

const PASSWORD_DE_PRUEBA = "Password123!";

const PELICULAS = [
  {
    nombre: "Interstellar",
    estreno: 2014,
    sinopsis: "Un grupo de exploradores viaja a traves de un agujero de gusano en busca de un nuevo hogar para la humanidad.",
    imagen: "https://image.tmdb.org/t/p/w500/gEU2QniE6E77NI6lCU6MxlNBvIe.jpg",
  },
  {
    nombre: "Inception",
    estreno: 2010,
    sinopsis: "Un ladron experto en robar secretos del subconsciente recibe la tarea de plantar una idea en la mente de alguien.",
    imagen: "https://image.tmdb.org/t/p/w500/oYuLEt3zVCKq57qu2F8dT7NIa6f.jpg",
  },
  {
    nombre: "The Matrix",
    estreno: 1999,
    sinopsis: "Un programador descubre que la realidad es una simulacion controlada por maquinas y se une a la resistencia.",
    imagen: "https://image.tmdb.org/t/p/w500/f89U3ADr1oiB1s9GkdPOEpXUk5H.jpg",
  },
  {
    nombre: "Pulp Fiction",
    estreno: 1994,
    sinopsis: "Historias interconectadas de gangsters, un boxeador y una pareja de ladrones en el Los Angeles criminal.",
    imagen: "https://image.tmdb.org/t/p/w500/d5iIlFn5s0ImszYzBPb8JPIfbXD.jpg",
  },
  {
    nombre: "The Godfather",
    estreno: 1972,
    sinopsis: "El patriarca de una familia mafiosa transfiere el control de su imperio a su hijo menor.",
    imagen: "https://image.tmdb.org/t/p/w500/3bhkrj58Vtu7enYsLMdL73KsPyd.jpg",
  },
  {
    nombre: "Forrest Gump",
    estreno: 1994,
    sinopsis: "Un hombre de Alabama con bajas capacidades intelectuales vive eventos historicos clave del siglo XX.",
    imagen: "https://image.tmdb.org/t/p/w500/arw2vcBveWOVZr6pxd9XTd1TdQa.jpg",
  },
  {
    nombre: "The Dark Knight",
    estreno: 2008,
    sinopsis: "Batman enfrenta al Joker, un criminal caotico que amenaza con hundir Gotham en la anarquia.",
    imagen: "https://image.tmdb.org/t/p/w500/qJ2tW6WMUDux911r6m7haRef0WH.jpg",
  },
  {
    nombre: "Fight Club",
    estreno: 1999,
    sinopsis: "Un insomne y un vendedor de jabon crean un club de lucha clandestino que deriva en algo mucho mas oscuro.",
    imagen: "https://image.tmdb.org/t/p/w500/pB8BM7pdSp6B6Ih7QZ4DrQ3PmJK.jpg",
  },
  {
    nombre: "Parasite",
    estreno: 2019,
    sinopsis: "Una familia pobre se infiltra en la vida de una adinerada familia de Seoul con consecuencias impredecibles.",
    imagen: "https://image.tmdb.org/t/p/w500/7IiTTgloJzvGI1TAYymCfbfl3vT.jpg",
  },
  {
    nombre: "Whiplash",
    estreno: 2014,
    sinopsis: "Un joven baterista ambicioso es llevado al limite por un instructor de musica obsesivo y despiadado.",
    imagen: "https://image.tmdb.org/t/p/w500/7fn624j5lj3xTme2SgiLCeuedmO.jpg",
  },
  {
    nombre: "Coco",
    estreno: 2017,
    sinopsis: "Un nino viaja al mundo de los muertos para descubrir los secretos de su familia y su pasion por la musica.",
    imagen: "https://image.tmdb.org/t/p/w500/gGEsBPAijhVUFoiNpgZXqRVWJt2.jpg",
  },
  {
    nombre: "Spirited Away",
    estreno: 2001,
    sinopsis: "Una nina queda atrapada en un mundo espiritista y debe trabajar en un bano magico para rescatar a sus padres.",
    imagen: "https://image.tmdb.org/t/p/w500/39wmItIWsg5sZMyRUHLkWBcuVCM.jpg",
  },
  {
    nombre: "The Shawshank Redemption",
    estreno: 1994,
    sinopsis: "Un banquero inocente encarcelado forge una amistad especial mientras busca recuperar su libertad.",
    imagen: "https://image.tmdb.org/t/p/w500/q6y0Go1tsGEsmtFryDOJo3dEmqu.jpg",
  },
  {
    nombre: "Gladiator",
    estreno: 2000,
    sinopsis: "Un general romano traicionado se convierte en gladiador y busca venganza contra el corrupto emperador.",
    imagen: "https://image.tmdb.org/t/p/w500/ty8TGRuvJLPUmAR1H1nRIsgwvim.jpg",
  },
  {
    nombre: "Titanic",
    estreno: 1997,
    sinopsis: "Una historia de amor entre dos jovenes de clases distintas a bordo del transatlantico Titanic.",
    imagen: "https://image.tmdb.org/t/p/w500/9xjZS2rlVxm8SFx8kPC3aIGCOYQ.jpg",
  },
  {
    nombre: "Avatar",
    estreno: 2009,
    sinopsis: "Un marine paraplejico viaja a Pandora, un exuberante planeta extraterrestre en conflicto con los humanos.",
    imagen: "https://image.tmdb.org/t/p/w500/jRXYjXNq0Cs2TcJjLkki24MLp7u.jpg",
  },
  {
    nombre: "Joker",
    estreno: 2019,
    sinopsis: "Arthur Fleck, un comico fracasado en Gotham, va perdiendo la cordura hasta convertirse en el Joker.",
    imagen: "https://image.tmdb.org/t/p/w500/udDclJoHjfjb8Ekgsd4FDteOkCU.jpg",
  },
  {
    nombre: "La La Land",
    estreno: 2016,
    sinopsis: "Una actriz aspirante y un musico de jazz se enamoran en Los Angeles mientras persiguen sus suenos.",
    imagen: "https://image.tmdb.org/t/p/w500/uDO8zWDhfWwoFdKS4fzkUJt0Rf0.jpg",
  },
  {
    nombre: "Get Out",
    estreno: 2017,
    sinopsis: "Un hombre afroamericano descubre oscuros secretos cuando visita a la familia de su novia blanca.",
    imagen: "https://image.tmdb.org/t/p/w500/tFXcEccSQMf3lfhfXKSU9iRBpa3.jpg",
  },
  {
    nombre: "Everything Everywhere All at Once",
    estreno: 2022,
    sinopsis: "Una lavandera descubre que puede acceder a vidas de universos paralelos y debe salvar el multiverso.",
    imagen: "https://image.tmdb.org/t/p/w500/w3LxiVYdWWRvEVdn5RYq6jIqkb1.jpg",
  },
];

async function main() {
  const passwordHasheada = await bcrypt.hash(PASSWORD_DE_PRUEBA, 10);

  const usuario = await prisma.usuario.upsert({
    where: { correo: "ana@ejemplo.com" },
    update: {},
    create: {
      correo: "ana@ejemplo.com",
      nombre: "Ana",
      password: passwordHasheada,
    },
  });

  const admin = await prisma.usuario.upsert({
    where: { correo: "admin@ejemplo.com" },
    update: {},
    create: {
      correo: "admin@ejemplo.com",
      nombre: "Admin",
      password: passwordHasheada,
      rol: "ADMIN",
    },
  });

  await prisma.pelicula.deleteMany({});

  await prisma.pelicula.createMany({
    data: PELICULAS.map((p) => ({
      nombre: p.nombre,
      estreno: p.estreno,
      sinopsis: p.sinopsis,
      imagen: p.imagen,
    })),
  });

  console.log("Usuarios de prueba:");
  console.log(`  ${usuario.correo} (rol: ${usuario.rol})`);
  console.log(`  ${admin.correo} (rol: ${admin.rol})`);
  console.log(`  Password para ambos: "${PASSWORD_DE_PRUEBA}"`);
  console.log(`${PELICULAS.length} peliculas creadas con sinopsis y posters reales.`);
}

main()
  .catch((e) => {
    console.error(e);
    process.exit(1);
  })
  .finally(() => prisma.$disconnect());