import {
  CanActivate,
  ExecutionContext,
  Injectable,
  UnauthorizedException,
} from '@nestjs/common';
import { Reflector } from '@nestjs/core';
import { ConfigService } from '@nestjs/config';
import { JwtService } from '@nestjs/jwt';
import { IS_PUBLIC_KEY } from '../decorators/public.decorator';

/**
 * Verifica el JWT en el header Authorization. Registrado como guard
 * GLOBAL (ver app.module.ts): por defecto TODO endpoint requiere token,
 * salvo que este marcado con @Public().
 */
@Injectable()
export class AuthGuard implements CanActivate {
  constructor(
    private readonly jwtService: JwtService,
    private readonly configService: ConfigService,
    private readonly reflector: Reflector,
  ) {}

  async canActivate(context: ExecutionContext): Promise<boolean> {
    // getAllAndOverride revisa el metodo primero y despues la clase:
    // permite marcar @Public() en un solo endpoint de un controller
    // que por lo demas esta protegido.
    const esPublica = this.reflector.getAllAndOverride<boolean>(IS_PUBLIC_KEY, [
      context.getHandler(),
      context.getClass(),
    ]);

    if (esPublica) return true;

    const request = context.switchToHttp().getRequest();
    const token = this.extraerToken(request);

    if (!token) {
      throw new UnauthorizedException('No se envio un token de acceso');
    }

    try {
      const payload = await this.jwtService.verifyAsync(token, {
        secret: this.configService.getOrThrow<string>('JWT_SECRET'),
      });
      // Queda disponible para @CurrentUser() y para el RolesGuard.
      request.user = payload;
    } catch {
      throw new UnauthorizedException('Token invalido o expirado');
    }

    return true;
  }

  private extraerToken(request: Record<string, any>): string | undefined {
    const authHeader: string | undefined = request.headers?.authorization;
    const [tipo, token] = authHeader?.split(' ') ?? [];
    return tipo === 'Bearer' ? token : undefined;
  }
}
