import { CanActivate, ExecutionContext, ForbiddenException, Injectable } from '@nestjs/common';
import { Reflector } from '@nestjs/core';
import { ROLES_KEY } from '../decorators/roles.decorator';
import { Rol } from '../../generated/prisma/enums';

/**
 * Autorizacion por rol. Se aplica DESPUES de AuthGuard (necesita
 * request.user, que AuthGuard ya puso ahi). Si la ruta no declara
 * @Roles(...), deja pasar a cualquier usuario autenticado.
 */
@Injectable()
export class RolesGuard implements CanActivate {
  constructor(private readonly reflector: Reflector) {}

  canActivate(context: ExecutionContext): boolean {
    const rolesRequeridos = this.reflector.getAllAndOverride<Rol[]>(ROLES_KEY, [
      context.getHandler(),
      context.getClass(),
    ]);

    if (!rolesRequeridos || rolesRequeridos.length === 0) return true;

    const { user } = context.switchToHttp().getRequest();

    if (!user || !rolesRequeridos.includes(user.rol)) {
      throw new ForbiddenException('No tienes permisos para esta accion');
    }

    return true;
  }
}
