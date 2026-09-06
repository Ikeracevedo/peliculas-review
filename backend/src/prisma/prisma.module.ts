import { Global, Module } from '@nestjs/common';
import { PrismaService } from './prisma.service';

/**
 * @global evita tener que importar el PrismaModule en cada modulo del proyecto
 * Se declara una sola vez y esto causa una sola instancia y una sola conexion
 */
@Global()
@Module({
    // Quien crea la instancia 
    providers:[PrismaService],
    // QUien puede llamarla 
    exports:[PrismaService],
})
export class PrismaModule {}