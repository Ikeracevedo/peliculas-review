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
    // no queda hardcodeado en el codigo fuente .
    JwtModule.registerAsync({
      imports: [ConfigModule],
      inject: [ConfigService],
      useFactory: (config: ConfigService) => ({
        secret: config.getOrThrow<string>('JWT_SECRET'),
        // expiresIn debe ser un numero de segundos
        signOptions: { expiresIn: Number(config.get<string>('JWT_EXPIRES_IN') ?? 3600) },
      }),
    }),
  ],
  providers: [
    AuthService,
    // Guard global: por defecto todo endpoint exige JWT.
    // Las rutas publicas se marcan con @Public().
    { provide: APP_GUARD, useClass: AuthGuard },
  ],
  controllers: [AuthController],
})
export class AuthModule {}
