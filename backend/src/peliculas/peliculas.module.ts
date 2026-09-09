import { Module } from '@nestjs/common';
import { PeliculasService } from './peliculas.service';
import { PeliculasController } from './peliculas.controller';

@Module({
  // Maneja los controladores del recurso
  controllers: [PeliculasController],
  // Maneja los servicios del recurso
  providers: [PeliculasService],
})

// Permite que la clase sea utilizada en otros modulos
export class PeliculasModule {}
