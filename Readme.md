<div align="center">
  <img src="docs/logo-upb.png" alt="Universidad Pontificia Bolivariana" width="340"/>

  <h1>CineVault — Reseñas de Películas (Migración a ASP.NET Core Web API)</h1>

  <p>Plataforma comunitaria para explorar un catálogo de películas, consultar las opiniones de otros usuarios y compartir calificaciones y reseñas.<br>
  Cada usuario puede registrarse, iniciar sesión y gestionar sus propias reseñas —crearlas, editarlas o eliminarlas— con su sesión protegida mediante <strong>JWT</strong>.</p>

  <p><sub>Arquitectura cliente-servidor desacoplada: SPA en Vue 3 + API REST stateless migrada de NestJS a <strong>ASP.NET Core Web API</strong> con Entity Framework Core y SQLite.</sub></p>

  <p>
    <img src="https://img.shields.io/badge/.NET-ASP.NET%20Core%20Web%20API-512BD4?logo=dotnet&logoColor=white" />
    <img src="https://img.shields.io/badge/EF%20Core-SQLite-003B57?logo=sqlite&logoColor=white" />
    <img src="https://img.shields.io/badge/JWT-Bearer%20Auth-000000?logo=jsonwebtokens&logoColor=white" />
    <img src="https://img.shields.io/badge/Swagger-OpenAPI-85EA2D?logo=swagger&logoColor=black" />
    <img src="https://img.shields.io/badge/Vue-3-4FC08D?logo=vue.js&logoColor=white" />
    <img src="https://img.shields.io/badge/Vite-Frontend-646CFF?logo=vite&logoColor=white" />
  </p>

  <p><strong>Plataforma de Programación Empresarial — Ingeniería de Sistemas e Informática · Universidad Pontificia Bolivariana</strong></p>
</div>

---

## 📋 Descripción del Proyecto (Migración de NestJS a ASP.NET Core)

Este proyecto consiste en la **migración completa del backend** de la plataforma de reseñas de películas **CineVault**, reemplazando la API original construida en **NestJS + Prisma** por una nueva implementación en **ASP.NET Core Web API + Entity Framework Core + SQLite** (`PeliculasReviewApi`), manteniendo **100% de compatibilidad de contrato** con el cliente existente en **Vue 3** (`frontend`).

### Aspectos clave garantizados en la migración

1. **Contrato HTTP idéntico**: Mismas rutas bajo el prefijo `/api` (`/api/auth`, `/api/peliculas`, `/api/reviews`, `/api/usuarios`), mismos verbos HTTP (`GET`, `POST`, `PATCH`/`PUT`, `DELETE`), mismos códigos de estado (`200`, `201`, `400`, `401`, `403`, `404`, `409`) y misma estructura de payloads y errores JSON.
2. **Serialización JSON en `camelCase` y Enums como texto**: Configurada globalmente con `JsonNamingPolicy.CamelCase` y `JsonStringEnumConverter` para que el enum `Rol` se serialice y deserialice exactamente como `"USUARIO"` o `"ADMIN"`, y el token de autenticación se entregue en la propiedad `access_token`.
3. **Búsqueda y paginación**: Implementadas en `GET /api/peliculas` y `GET /api/reviews` mediante parámetros de consulta opcionales (`nombre`/`search`, `page`, `limit`/`pageSize`), exponiendo los metadatos en las cabeceras `X-Total-Count`, `X-Page`, `X-Limit` y `X-Total-Pages` sin alterar el arreglo plano esperado por el frontend.
4. **Política de CORS para Vue**: Configurada en `Program.cs` (`FrontendVue`) permitiendo el origen del cliente Vite (`http://localhost:5173`), credenciales (`AllowCredentials`), todos los encabezados y métodos HTTP.
5. **Seguridad con BCrypt y JWT**:
   - Contraseñas hasheadas con **BCrypt.Net-Next** (`workFactor: 10`) antes de persistirse en SQLite.
   - Emisión de tokens **JWT firmados con HMAC-SHA256** (`sub`, `correo`, `rol`, `iat`, `exp`).
   - Endpoints de lectura (`GET /api/peliculas`, `GET /api/reviews`) de acceso público y endpoints de escritura (`POST`, `PUT`, `PATCH`, `DELETE`) protegidos con `[Authorize]`.
   - Control de propiedad (*ownership*) en reseñas (solo el autor o un `ADMIN` puede editar/eliminar) y panel `/api/usuarios` restringido a `[Authorize(Roles = "ADMIN")]`.
6. **Swagger / OpenAPI con botón "Authorize"**: Documentación interactiva en `/swagger` configurada con `AddSecurityDefinition` y `AddSecurityRequirement` para probar endpoints protegidos pegando el token JWT directamente en la interfaz gráfica.

---

## 🛠️ Tecnologías

| Capa | Tecnología | Rol |
|---|---|---|
| **Backend** | ASP.NET Core Web API (C#) | API REST stateless |
| **ORM** | Entity Framework Core (`Microsoft.EntityFrameworkCore.Sqlite`) | Modelado de datos, migraciones y consultas LINQ |
| **Base de datos** | SQLite (`peliculas.db`) | Persistencia relacional local sin servidor externo |
| **Autenticación** | `Microsoft.AspNetCore.Authentication.JwtBearer` + `BCrypt.Net-Next` | Hash de contraseñas y sesiones con JWT firmado |
| **Documentación API** | `Swashbuckle.AspNetCore` (Swagger UI + OpenAPI) | Explorador interactivo con autenticación Bearer |
| **Frontend** | Vue 3 (`<script setup>`) + Vite + Pinia + Vue Router + Axios + Tailwind CSS | Single Page Application (SPA) |

---

## 📂 Estructura del Repositorio

```text
peliculas-review/
├── PeliculasReviewApi/          # Nuevo Backend en ASP.NET Core Web API
│   ├── Controllers/             # Controladores REST (Auth, Peliculas, Reviews, Usuarios, App)
│   ├── Data/                    # AppDbContext, DbSeeder (seed automático) e interceptores SQLite
│   ├── Dtos/                    # DTOs de entrada/salida y consultas paginadas
│   ├── Migrations/              # Migraciones de Entity Framework Core
│   ├── Models/                  # Entidades del dominio (Usuario, Pelicula, Review, Rol)
│   ├── Services/                # Lógica de negocio desacoplada por módulo
│   ├── Validation/              # Validadores personalizados y metadatos camelCase
│   ├── appsettings.json         # Cadena de conexión SQLite, configuración JWT y orígenes CORS
│   └── Program.cs               # Configuración de DI, EF Core, JWT, JSON, CORS, Swagger y pipeline HTTP
├── frontend/                    # Cliente SPA en Vue 3 + Vite (CineVault)
│   ├── src/
│   │   ├── api/index.ts         # Instancia Axios e interceptores JWT
│   │   ├── router/              # Rutas y Navigation Guards
│   │   ├── stores/              # Estado global con Pinia (auth, watchlist)
│   │   ├── types/               # Interfaces TypeScript
│   │   └── views/               # Vistas de catálogo, detalle, login/registro y administración
│   ├── .env                     # Variable VITE_API_URL apuntando al backend .NET
│   └── .env.example             # Plantilla de variables de entorno del frontend
└── backend/                     # Backend original de referencia en NestJS
```

---

## ✅ Requisitos Previos

Antes de ejecutar el proyecto, asegúrate de tener instalado en tu equipo:

1. **.NET SDK** (.NET 8.0 SDK o superior; proyecto configurado y probado sobre .NET SDK 8/9/10).
   - Verificar instalación:
     ```bash
     dotnet --version
     ```
2. **Herramienta CLI de Entity Framework Core (`dotnet-ef`)** (para ejecutar comandos de migración manualmente):
   ```bash
   dotnet tool install --global dotnet-ef
   ```
   *(Si ya la tienes instalada, puedes actualizarla con `dotnet tool update --global dotnet-ef`).*
3. **Node.js** (v18 o superior) y **npm** o **pnpm** (para ejecutar el cliente de Vue).

---

## 🚀 Paso a Paso: Levantar el Backend (ASP.NET Core Web API)

### 1. Ubicarse en la carpeta del proyecto .NET y restaurar paquetes

```bash
cd PeliculasReviewApi
dotnet restore
```

### 2. Aplicar las migraciones de Entity Framework Core en SQLite

Ejecuta el siguiente comando para crear la base de datos local `peliculas.db` y aplicar el esquema de tablas (`Usuarios`, `Peliculas`, `Reviews`):

```bash
dotnet ef database update
```

> 💡 **Nota sobre datos de prueba (Seed automático)**: Al iniciar la aplicación, `DbSeeder.SeedAsync` ejecuta automáticamente `Database.MigrateAsync()` y, si la base de datos está vacía, inserta los **2 usuarios de prueba** y el **catálogo inicial de 20 películas**.

### 3. Iniciar el servidor de la API

```bash
dotnet run
```

La API quedará escuchando en:
- **URL Base de la API**: `http://localhost:5000/api`
- **Interfaz gráfica Swagger UI**: `http://localhost:5000/swagger`
- **Documento OpenAPI JSON**: `http://localhost:5000/swagger/v1/swagger.json`

### 4. Probar la autenticación JWT desde Swagger UI

1. Abre `http://localhost:5000/swagger` en tu navegador.
2. Ejecuta `POST /api/auth/login` con las credenciales de prueba (por ejemplo, `admin@ejemplo.com` / `Password123!`).
3. Copia el valor de `access_token` de la respuesta.
4. Haz clic en el botón **"Authorize"** (arriba a la derecha en Swagger UI), pega el token en el campo **Value** y pulsa **Authorize**.
5. Ahora podrás probar todos los endpoints protegidos (`POST`, `PUT`, `PATCH`, `DELETE` y `/api/usuarios`).

---

## 🔗 Configuración y Conexión del Frontend de Vue

Para conectar el cliente de Vue al nuevo backend en ASP.NET Core en lugar del servidor anterior de NestJS (que usaba el puerto `3000`), se debe configurar la variable de entorno **`VITE_API_URL`** en el archivo `frontend/.env`.

### 1. Configurar la variable de entorno en `frontend/.env`

Crea o edita el archivo `frontend/.env` en la raíz de la carpeta `frontend/` con el siguiente contenido explícito:

```env
VITE_API_URL=http://localhost:5000/api
```

*(En `frontend/src/api/index.ts`, Axios lee `import.meta.env.VITE_API_URL` para dirigir todas las peticiones HTTP hacia `http://localhost:5000/api`).*

### 2. Instalar dependencias y levantar el cliente Vue

En una nueva terminal, ejecuta:

```bash
cd frontend
npm install
npm run dev
```
*(O con pnpm: `pnpm install && pnpm dev`).*

El cliente de Vue estará disponible en:
- **Frontend (Vite)**: `http://localhost:5173`

---

## 🔑 Credenciales de Prueba (Seed Inicial)

| Rol | Correo | Contraseña | Permisos |
|---|---|---|---|
| **Usuario estándar (`USUARIO`)** | `ana@ejemplo.com` | `Password123!` | Explorar catálogo, publicar reseñas y editar/eliminar sus propias reseñas |
| **Administrador (`ADMIN`)** | `admin@ejemplo.com` | `Password123!` | Todo lo anterior + CRUD de películas (`/admin/peliculas`), moderación de cualquier reseña y gestión de usuarios (`/admin/usuarios`) |

---

## 📡 Resumen de Endpoints de la API

### Autenticación (`/api/auth`)

| Verbo | Ruta | Acceso | Descripción |
|---|---|---|---|
| `POST` | `/api/auth/registro` | Público | Registra un nuevo usuario (contraseña hasheada con BCrypt, rol `USUARIO` por defecto) y retorna `{ usuario, access_token }`. |
| `POST` | `/api/auth/login` | Público | Valida credenciales y retorna `{ usuario, access_token }` firmado con JWT. |

### Películas (`/api/peliculas`)

| Verbo | Ruta | Acceso | Descripción |
|---|---|---|---|
| `GET` | `/api/peliculas` | Público | Lista películas ordenadas por `creadaEn` desc. Soporta `?nombre=`, `?search=`, `?page=` y `?limit=`/`?pageSize=`. |
| `GET` | `/api/peliculas/{id}` | Público | Obtiene el detalle de una película por su ID. |
| `POST` | `/api/peliculas` | 🔒 `[Authorize]` | Crea una nueva película. |
| `PATCH` / `PUT` | `/api/peliculas/{id}` | 🔒 `[Authorize]` | Actualiza los datos de una película existente. |
| `DELETE` | `/api/peliculas/{id}` | 🔒 `[Authorize]` | Elimina una película y sus reseñas asociadas en cascada. |

### Reseñas (`/api/reviews`)

| Verbo | Ruta | Acceso | Descripción |
|---|---|---|---|
| `GET` | `/api/reviews` | Público | Lista reseñas con `pelicula` y `autor` anidados. Soporta `?nombre=`, `?search=`, `?titulo=`, `?peliculaId=`, `?autorId=`, `?page=` y `?limit=`/`?pageSize=`. |
| `GET` | `/api/reviews/{id}` | Público | Obtiene una reseña por su ID con `pelicula` y `autor`. |
| `POST` | `/api/reviews` | 🔒 `[Authorize]` | Crea una reseña asociando automáticamente el `autorId` desde el claim `sub` del JWT. |
| `PATCH` / `PUT` | `/api/reviews/{id}` | 🔒 `[Authorize]` (Autor o `ADMIN`) | Actualiza una reseña (valida propiedad o rol `ADMIN`). |
| `DELETE` | `/api/reviews/{id}` | 🔒 `[Authorize]` (Autor o `ADMIN`) | Elimina una reseña (valida propiedad o rol `ADMIN`). |

### Usuarios (`/api/usuarios`)

| Verbo | Ruta | Acceso | Descripción |
|---|---|---|---|
| `GET` | `/api/usuarios` | 🔒 `[Authorize(Roles = "ADMIN")]` | Lista todos los usuarios registrados (sin exponer `password`). |
| `GET` | `/api/usuarios/{id}` | 🔒 `[Authorize(Roles = "ADMIN")]` | Obtiene un usuario por su ID. |
| `PATCH` / `PUT` | `/api/usuarios/{id}` | 🔒 `[Authorize(Roles = "ADMIN")]` | Actualiza nombre, correo o rol (`USUARIO` / `ADMIN`) de un usuario. |
| `DELETE` | `/api/usuarios/{id}` | 🔒 `[Authorize(Roles = "ADMIN")]` | Elimina un usuario y sus reseñas en cascada. |

---

## 👥 Equipo

| Integrante |
|---|
| Iker Acevedo |
| Jose Mejía |

Ingeniería de Sistemas e Informática · Universidad Pontificia Bolivariana  
Plataforma de Programación Empresarial
