-- CreateTable
CREATE TABLE "Usuario" (
    "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    "correo" TEXT NOT NULL,
    "nombre" TEXT NOT NULL,
    "password" TEXT NOT NULL,
    "creadoEn" DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "actualizadoEn" DATETIME NOT NULL
);

-- CreateTable
CREATE TABLE "Pelicula" (
    "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    "nombre" TEXT NOT NULL,
    "imagen" TEXT NOT NULL,
    "descripcion" TEXT,
    "estreno" INTEGER,
    "creadaEn" DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "actualizadoEn" DATETIME NOT NULL
);

-- CreateTable
CREATE TABLE "Review" (
    "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    "titulo" TEXT NOT NULL,
    "contenido" TEXT NOT NULL,
    "calificacion" INTEGER NOT NULL,
    "creadaEn" DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "actualizadoEn" DATETIME NOT NULL,
    "peliculaId" INTEGER NOT NULL,
    "autorId" INTEGER NOT NULL,
    CONSTRAINT "Review_peliculaId_fkey" FOREIGN KEY ("peliculaId") REFERENCES "Pelicula" ("id") ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT "Review_autorId_fkey" FOREIGN KEY ("autorId") REFERENCES "Usuario" ("id") ON DELETE CASCADE ON UPDATE CASCADE
);

-- CreateIndex
CREATE UNIQUE INDEX "Usuario_correo_key" ON "Usuario"("correo");

-- CreateIndex
CREATE INDEX "Review_peliculaId_idx" ON "Review"("peliculaId");

-- CreateIndex
CREATE INDEX "Review_autorId_idx" ON "Review"("autorId");

-- CreateIndex
CREATE UNIQUE INDEX "Review_peliculaId_autorId_key" ON "Review"("peliculaId", "autorId");
