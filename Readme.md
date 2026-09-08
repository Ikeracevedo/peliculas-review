<div align="center">
  <img src="docs/logo-upb.png" alt="Universidad Pontificia Bolivariana" width="340"/>

  <h1>Reseñas de Películas</h1>

  <p>Una comunidad para explorar un catálogo de películas, lee lo que<br>
  otros usuarios opinan y comparte tu propia calificación y reseña de cada una.<br>
  Cada usuario se registra, inicia sesión y gestiona sus propias reseñas —<br>
  crearlas, editarlas o eliminarlas — con su sesión protegida mediante JWT.</p>

  <p><sub>Arquitectura cliente-servidor desacoplada: SPA en Vue + API REST stateless en NestJS.</sub></p>

  <p>
    <img src="https://img.shields.io/badge/NestJS-12-E0234E?logo=nestjs&logoColor=white" />
    <img src="https://img.shields.io/badge/Prisma-7-2D3748?logo=prisma&logoColor=white" />
    <img src="https://img.shields.io/badge/SQLite-database-003B57?logo=sqlite&logoColor=white" />
    <img src="https://img.shields.io/badge/Vue-3-4FC08D?logo=vue.js&logoColor=white" />
    <img src="https://img.shields.io/badge/JWT-auth-000000?logo=jsonwebtokens&logoColor=white" />
    <img src="https://img.shields.io/badge/pnpm-package%20manager-F69220?logo=pnpm&logoColor=white" />
  </p>

  <p><strong>Plataforma de Programación Empresarial — Ingeniería de Sistemas e Informática · Universidad Pontificia Bolivariana</strong></p>
</div>

---

## ¿Qué puedes hacer?

EL objetivo es crear una comunidad para:

- 🔎 **Explorar** un catálogo de películas con imagen, nombre y año de estreno.
- ✍️ **Reseñar** cualquier película: título, contenido y una calificación de 1 a 5.
- 👤 **Registrarse e iniciar sesión** de forma segura (contraseña hasheada, sesión con JWT).
- ✏️ **Editar o eliminar** únicamente sus propias reseñas — nunca las de otro usuario. Un `ADMIN` sí puede moderar cualquiera.
- 📖 **Leer** las reseñas de todos, con el nombre del autor y la película asociada.

---

## Tecnologías

| Capa | Tecnología | Rol |
|---|---|---|
| Backend | NestJS 12 (TypeScript, CommonJS) | API REST |
| ORM | Prisma 7 (driver adapter `better-sqlite3`) | Acceso a datos tipado |
| Base de datos | SQLite | Persistencia local, sin servidor externo |
| Autenticación | `@nestjs/jwt` + `bcrypt` | Sesión stateless con tokens firmados |
| Frontend | Vue 3 + Vite | SPA que consume la API |
| Gestor de paquetes | pnpm | Instalación estricta, sin dependencias fantasma |

---

## Arquitectura

El proyecto es un **monorepo de dos carpetas hermanas**, sin herramienta de monorepo (no comparten dependencias, así que no la necesitan):

```
peliculas-review/
├── backend/     # API REST en NestJS — este README la documenta en detalle
└── frontend/    # SPA en Vue — consume la API, ver sección Frontend
```

### Por qué esta arquitectura

- **Cliente-servidor desacoplado**: el backend nunca devuelve HTML, solo JSON. El frontend arma toda la vista en el navegador (SPA).
- **HTTP es stateless**: cada petición llega sin memoria de la anterior. La sesión se resuelve con **JWT**, no con cookies de sesión en el servidor — así el backend no necesita guardar estado entre peticiones.
- **Backend en capas** (dentro de cada módulo):

  ```
  HTTP Request
      ↓
  Controller   → solo sabe de HTTP: rutas, status codes, DTOs de entrada/salida
      ↓
  Service      → reglas de negocio, no sabe que existe HTTP
      ↓
  PrismaService → acceso a datos, una sola instancia para toda la app (@Global)
      ↓
  SQLite
  ```

---

## Frontend — guía para el equipo

> Aún no se ha creado el proyecto. Stack acordado: **Vue 3 + Vite**.

Definir estructura del proyecto y decisiones clave para el desarrollo

--- 

## Backend — cómo funciona

### Estructura

```
backend/
├── prisma/
│   ├── schema.prisma       # Modelos: Usuario, Pelicula, Review
│   ├── migrations/         # Historial versionado de cambios a la base
│   └── seed.ts             # 2 usuarios + 20 películas de prueba
├── src/
│   ├── prisma/             # PrismaModule (@Global) + PrismaService
│   ├── usuarios/           # Persistencia + CRUD de administración (solo ADMIN)
│   ├── auth/               # Registro, login, JWT, guards, decoradores
│   ├── peliculas/          # CRUD de películas
│   ├── reviews/            # CRUD de reseñas (con ownership)
│   ├── app.module.ts
│   └── main.ts             # Bootstrap: CORS, prefijo /api, ValidationPipe global
├── requests/                # Archivos .http para probar la API desde VS Code
├── docs/                    # Documentación de la API para el equipo (ver docs/API.md)
└── prisma7.config.ts        # Config de Prisma (migraciones, seed)
```

### Decisiones clave (por si el equipo se pregunta "por qué así")

- **`PrismaModule` es `@Global()`**: una sola conexión a SQLite para toda la app. Si cada módulo declarara su propio `PrismaService`, tendríamos múltiples conexiones abriéndose a la misma base — un antipatrón real en Nest.
- **Guard global de autenticación**: toda la API exige JWT salvo `/auth/registro` y `/auth/login`. Las rutas públicas se marcan explícitamente con `@Public()`.
- **`usuarios/` es solo para administración**: no hay endpoint público de registro/listado de usuarios ahí — eso vive en `auth/`. `GET/PATCH/DELETE /usuarios` requieren rol `ADMIN`.
- **Ownership en reviews**: un usuario con rol `USUARIO` solo puede editar o borrar sus propias reseñas; un `ADMIN` puede moderar cualquiera. El `autorId` nunca viaja en el body — sale del JWT, para que nadie pueda publicar reseñas fingiendo ser otro usuario.
- **Todo el schema en español** (`Usuario`, `Pelicula`), con `Review` en inglés a propósito — decisión consciente del equipo, no inconsistencia.
- **`ValidationPipe` global** con `whitelist: true`: cualquier campo que el cliente mande y no esté declarado en el DTO se descarta automáticamente (evita mass assignment).

### Variables de entorno (`backend/.env`, no se sube a git)

```bash
DATABASE_URL="file:./dev.db"
JWT_SECRET="<cadena aleatoria larga — generar con: node -e "console.log(require('crypto').randomBytes(64).toString('hex'))">"
JWT_EXPIRES_IN="3600"
```

### Cómo levantar el backend

```bash
cd backend
pnpm install
pnpm exec prisma migrate dev   # crea/actualiza dev.db según prisma/schema.prisma
pnpm exec prisma db seed       # crea una base para trabajar con datos de prueba
pnpm start:dev                 # Ruta http://localhost:3000/api
```

### Scripts útiles

```bash
pnpm start:dev            # servidor con recarga automática
pnpm build                # compila a dist/
pnpm test                 # tests unitarios (vitest)
pnpm test:e2e             # tests end-to-end
pnpm exec prisma studio   # explorador visual de la base de datos
```

---

### Cómo conectarse a la API

- Base URL en desarrollo: `http://localhost:3000/api`
- CORS ya está habilitado para `http://localhost:5173` (puerto por defecto de Vite). Si usan otro puerto, avisar para agregarlo en `backend/src/main.ts`.
- Formato de error de validación: `400 Bad Request` con el detalle de qué campo falló.

### Autenticación — cómo funciona (ya está lista, no es temporal)

Casi todos los endpoints requieren sesión. Las únicas rutas públicas son `POST /auth/registro` y `POST /auth/login` — todo lo demás responde `401 Unauthorized` sin un token válido.

1. Te registras (`/auth/registro`) o inicias sesión (`/auth/login`).
2. La respuesta trae `access_token` (un JWT). Guárdalo en el cliente (`localStorage`, una store de Pinia, etc.).
3. En cada petición protegida, lo mandas en el header: `Authorization: Bearer <access_token>`.
4. El token expira en 1 hora — pasado ese tiempo cualquier petición da `401` y hay que volver a hacer login.
5. `/usuarios` además exige rol `ADMIN`: con token válido pero sin ese rol, responde `403 Forbidden` en vez de `401`.
6. El endpoint de reseñas (`POST /reviews`) **ya no pide `autorId` en el body** — el autor sale automáticamente del token.

Usa las credenciales del seed para probar sin registrarte: `ana@ejemplo.com` / `Password123!` (usuario normal) o `admin@ejemplo.com` / `Password123!` (administrador).

📖 Ejemplo de `fetch` con login + petición protegida, y el detalle completo de cada endpoint (body, respuestas, errores), en [`backend/docs/API.md`](backend/docs/API.md) — léelo antes de tipar los modelos en el front.

### Cómo levantar el frontend (una vez creado)

```bash
cd frontend
pnpm install
pnpm dev   # http://localhost:5173
```

---

## Endpoints principales

### Autenticación — `/auth`

| Método | Ruta | Requiere | Descripción |
|---|---|---|---|
| POST | `/api/auth/registro` | — | Registrar usuario (rol `USUARIO` por defecto) |
| POST | `/api/auth/login` | — | Iniciar sesión, obtener `access_token` (JWT) |

### Películas — `/peliculas`

| Método | Ruta | Requiere | Descripción |
|---|---|---|---|
| POST | `/api/peliculas` | Token | Crear película |
| GET | `/api/peliculas` | Token | Listar películas |
| GET | `/api/peliculas/:id` | Token | Obtener película |
| PATCH | `/api/peliculas/:id` | Token | Actualizar película |
| DELETE | `/api/peliculas/:id` | Token | Eliminar película |

### Reseñas — `/reviews`

| Método | Ruta | Requiere | Descripción |
|---|---|---|---|
| POST | `/api/reviews` | Token | Crear reseña (el autor sale del token, no del body) |
| GET | `/api/reviews` | Token | Listar reseñas (con película y autor anidados) |
| GET | `/api/reviews/:id` | Token | Obtener reseña |
| PATCH | `/api/reviews/:id` | Token + dueño | Actualizar reseña propia |
| DELETE | `/api/reviews/:id` | Token + dueño | Eliminar reseña propia |

### Usuarios — `/usuarios` (administración)

| Método | Ruta | Requiere | Descripción |
|---|---|---|---|
| GET | `/api/usuarios` |  Token +  rol `ADMIN` | Listar usuarios |
| GET | `/api/usuarios/:id` |  Token +  rol `ADMIN` | Obtener usuario |
| PATCH | `/api/usuarios/:id` |  Token +  rol `ADMIN` | Actualizar usuario (incluye cambiar rol) |
| DELETE | `/api/usuarios/:id` |  Token +  rol `ADMIN` | Eliminar usuario |

---

## Equipo


| Integrante |
|---|
| Iker Acevedo | 
| Jose Mejía | 

Ingeniería de Sistemas e Informática · Universidad Pontificia Bolivariana
Plataforma de Programación Empresarial