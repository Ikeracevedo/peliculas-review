/*
  Warnings:

  - You are about to drop the column `descripcion` on the `Pelicula` table. All the data in the column will be lost.

*/
-- RedefineTables
PRAGMA defer_foreign_keys=ON;
PRAGMA foreign_keys=OFF;
CREATE TABLE "new_Pelicula" (
    "id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    "nombre" TEXT NOT NULL,
    "imagen" TEXT NOT NULL,
    "sinopsis" TEXT,
    "estreno" INTEGER,
    "creadaEn" DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "actualizadoEn" DATETIME NOT NULL
);
INSERT INTO "new_Pelicula" ("actualizadoEn", "creadaEn", "estreno", "id", "imagen", "nombre") SELECT "actualizadoEn", "creadaEn", "estreno", "id", "imagen", "nombre" FROM "Pelicula";
DROP TABLE "Pelicula";
ALTER TABLE "new_Pelicula" RENAME TO "Pelicula";
PRAGMA foreign_keys=ON;
PRAGMA defer_foreign_keys=OFF;
