/*
  Warnings:

  - You are about to drop the column `creadaEn` on the `Review` table. All the data in the column will be lost.

*/
-- RedefineTables
PRAGMA defer_foreign_keys=ON;
PRAGMA foreign_keys=OFF;
CREATE TABLE "new_Review" (
    "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    "titulo" TEXT NOT NULL,
    "contenido" TEXT NOT NULL,
    "calificacion" INTEGER NOT NULL,
    "creadoEn" DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "actualizadoEn" DATETIME NOT NULL,
    "peliculaId" INTEGER NOT NULL,
    "autorId" INTEGER NOT NULL,
    CONSTRAINT "Review_peliculaId_fkey" FOREIGN KEY ("peliculaId") REFERENCES "Pelicula" ("id") ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT "Review_autorId_fkey" FOREIGN KEY ("autorId") REFERENCES "Usuario" ("id") ON DELETE CASCADE ON UPDATE CASCADE
);
INSERT INTO "new_Review" ("actualizadoEn", "autorId", "calificacion", "contenido", "id", "peliculaId", "titulo") SELECT "actualizadoEn", "autorId", "calificacion", "contenido", "id", "peliculaId", "titulo" FROM "Review";
DROP TABLE "Review";
ALTER TABLE "new_Review" RENAME TO "Review";
CREATE INDEX "Review_peliculaId_idx" ON "Review"("peliculaId");
CREATE INDEX "Review_autorId_idx" ON "Review"("autorId");
CREATE UNIQUE INDEX "Review_peliculaId_autorId_key" ON "Review"("peliculaId", "autorId");
PRAGMA foreign_keys=ON;
PRAGMA defer_foreign_keys=OFF;
