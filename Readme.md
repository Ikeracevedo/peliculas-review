<div align="center">
  <img src="docs/logo-upb.png" alt="Universidad Pontificia Bolivariana" width="340"/>

  <h1>🎬 Reseñas de Películas</h1>

  <p><strong>¿Viste una película que te voló la cabeza? ¿O una que no vale ni el pop corn?<br>
  Dilo, fírmalo, y que el mundo lo sepa.</strong></p>

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

## La idea

Olvídate de las estrellitas genéricas de un sitio cualquiera. Aquí cada reseña **tiene nombre y apellido**: es tuya, queda guardada bajo tu cuenta, y solo tú puedes volver a tocarla — editarla cuando cambies de opinión, o borrarla si ya no representa lo que piensas.

Es una comunidad chica pero con reglas de una app real: cuentas seguras con contraseñas hasheadas (nunca en texto plano), sesiones firmadas con JWT que no dependen de que el servidor "recuerde" quién eres, y un catálogo de películas que crece con cada reseña nueva.

**Lo que vas a poder hacer:**

| | |
|---|---|
| 🎥 **Explorar** | Un catálogo de películas con imagen, título y año — el punto de partida de todo. |
| ✍️ **Opinar** | Publica tu reseña con una calificación de 1 a 5. Sin filtros, sin anonimato. |
| 🔐 **Tener cuenta propia** | Regístrate, inicia sesión, y que tu sesión quede protegida de principio a fin. |
| ✏️ **Tener el control** | Edita o borra *tus* reseñas cuando quieras. Las de los demás, ni las tocas. |
| 📖 **Leer a los demás** | Cada reseña muestra quién la escribió y sobre qué película — sin datos de más. |

Por debajo, es una API REST real construida sobre NestJS, con Prisma hablándole a SQLite y JWT cuidando cada puerta. Por encima, va a ser una SPA en Vue tan rápida que se siente como una app nativa. Bienvenido — dale un vistazo al código, no muerde.

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
Taller-1/
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

## Backend — cómo funciona

### Estructura

```
backend/
├── prisma/
│   ├── schema.prisma       # Modelos: Usuario, Pelicula, Review
│   ├── migrations/         # Historial versionado de cambios a la base
│   └── seed.ts             # Datos de prueba (usuario + película demo)
├── src/
│   ├── prisma/             # PrismaModule (@Global) + PrismaService
│   ├── usuarios/           # Persistencia de usuarios (sin controller público)
│   ├── auth/               # Registro, login, JWT, guards
│   ├── peliculas/          # CRUD de películas
│   ├── reviews/            # CRUD de reseñas
│   ├── app.module.ts
│   └── main.ts             # Bootstrap: CORS, prefijo /api, ValidationPipe global
├── requests/                # Archivos .http para probar la API desde VS Code
├── docs/                    # Documentación de la API para el equipo
└── prisma7.config.ts        # Config de Prisma (migraciones, seed)
```

### Decisiones clave (por si el equipo se pregunta "por qué así")

- **`PrismaModule` es `@Global()`**: una sola conexión a SQLite para toda la app. Si cada módulo declarara su propio `PrismaService`, tendríamos múltiples conexiones abriéndose a la misma base — un antipatrón real en Nest.
- **`usuarios/` no tiene controller**: no hay un endpoint público de listado/edición de usuarios. Solo `AuthService` lo consume internamente. Evita exponer correos y datos de cuentas sin protección.
- **Todo el schema en español** (`Usuario`, `Pelicula`), con `Review` en inglés a propósito — decisión consciente del equipo, no inconsistencia.
- **`ValidationPipe` global** con `whitelist: true`: cualquier campo que el cliente mande y no esté declarado en el DTO se descarta automáticamente (evita mass assignment).

### Variables de entorno (`backend/.env`, no se sube a git)

```bash
DATABASE_URL="file:./dev.db"
JWT_SECRET="<cadena aleatoria larga — generar con: node -e "console.log(require('crypto').randomBytes(64).toString('hex'))">"
JWT_EXPIRES_IN="1h"
```

### Cómo levantar el backend

```bash
cd backend
pnpm install
pnpm exec prisma migrate dev   # crea/actualiza dev.db según prisma/schema.prisma
pnpm exec prisma db seed       # datos de prueba
pnpm start:dev                 # http://localhost:3000/api
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

## Frontend — guía para el equipo

> 🚧 Aún no se ha creado el proyecto. Stack acordado: **Vue 3 + Vite**.

### Cómo conectarse a la API

- Base URL en desarrollo: `http://localhost:3000/api`
- CORS ya está habilitado para `http://localhost:5173` (puerto por defecto de Vite). Si usan otro puerto, avisar para agregarlo en `backend/src/main.ts`.
- Formato de error de validación: `400 Bad Request` con el detalle de qué campo falló.
- Autenticación (cuando esté lista): header `Authorization: Bearer <token>`.

### Lo que necesitan saber antes de construir pantallas

- El endpoint de reseñas (`POST /reviews`) hoy pide `autorId` en el body **de forma temporal** — va a desaparecer en cuanto se conecte la autenticación. No construir un input para eso.
- Revisar `backend/docs/` y `backend/requests/*.http` para ver la forma exacta de cada respuesta antes de tipar los modelos en el front.

### Cómo levantar el frontend (una vez creado)

```bash
cd frontend
pnpm install
pnpm dev   # http://localhost:5173
```

---

## Endpoints principales

| Método | Ruta | Descripción | Estado |
|---|---|---|---|
| POST | `/api/peliculas` | Crear película | ✅ |
| GET | `/api/peliculas` | Listar películas | ✅ |
| GET | `/api/peliculas/:id` | Obtener película | ✅ |
| PATCH | `/api/peliculas/:id` | Actualizar película | ✅ |
| DELETE | `/api/peliculas/:id` | Eliminar película | ✅ |
| POST | `/api/reviews` | Crear reseña | ✅ |
| GET | `/api/reviews` | Listar reseñas (con película y autor) | ✅ |
| GET | `/api/reviews/:id` | Obtener reseña | ✅ |
| PATCH | `/api/reviews/:id` | Actualizar reseña | ✅ |
| DELETE | `/api/reviews/:id` | Eliminar reseña | ✅ |
| POST | `/api/auth/registro` | Registrar usuario | 🚧 |
| POST | `/api/auth/login` | Iniciar sesión, obtener JWT | 🚧 |

> Detalle completo de cada endpoint (body, respuestas, errores) en `backend/docs/`.

---

## Equipo

<!-- Agregar/editar antes de subir a git -->

| Integrante | Rol |
|---|---|
| Iker Acevedo | Backend (NestJS, Prisma, Auth) |
| _[Nombre del compañero]_ | Frontend (Vue) |

Ingeniería de Sistemas e Informática · Universidad Pontificia Bolivariana
Plataforma de Programación Empresarial
