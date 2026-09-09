import { Module } from '@nestjs/common';
import { UsuariosService } from './usuarios.service';
import { UsuariosController } from './usuarios.controller';

@Module({
  // Capa para que lo consumas las peticiones HTTP
  controllers: [UsuariosController],
  // Capa de servicio y logica del negocio
  providers: [UsuariosService],
  // Permite que cualquier componente pueda importar y usar la logica del servicio
  exports: [UsuariosService], 
})

// Permite que la clase sea utilizada en otros modulos
export class UsuariosModule {}
