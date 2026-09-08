import { Module } from '@nestjs/common';
import { APP_GUARD } from '@nestjs/core';
import { ConfigModule, ConfigService } from '@nestjs/config';
import { JwtModule } from '@nestjs/jwt';
import { AuthService } from './auth.service';
import { AuthController } from './auth.controller';
import { UsuariosModule } from '../usuarios/usuarios.module';
import { AuthGuard } from './guards/auth.guard';

@Module({
  imports: [
    UsuariosModule,
    // registerAsync en vez de register(): el secreto sale de ConfigService,
    // no queda hardcodeado en el codigo fuente (a diferencia del ejemplo
    // del profesor, que trae el secreto en un archivo commiteado).
    JwtModule.registerAsync({
      imports: [ConfigModule],
      inject: [ConfigService],
      useFactory: (config: ConfigService) => ({
        secret: config.getOrThrow<string>('JWT_SECRET'),
        // expiresIn debe ser un numero de SEGUNDOS o un string con el
        // formato exacto que espera la libreria "ms" (ej. '1h', '7d').
        // Un string generico como el que devuelve ConfigService.get<string>()
        // no encaja en ese tipo restringido -> se normaliza a numero.
        signOptions: { expiresIn: Number(config.get<string>('JWT_EXPIRES_IN') ?? 3600) },
      }),
    }),
  ],
  providers: [
    AuthService,
    // Guard GLOBAL: por defecto TODO endpoint exige JWT.
    // Las rutas publicas se marcan con @Public().
    { provide: APP_GUARD, useClass: AuthGuard },
  ],
  controllers: [AuthController],
  exports: [JwtModule], // AuthGuard, en otro modulo, necesita inyectar JwtService
})
export class AuthModule {}
