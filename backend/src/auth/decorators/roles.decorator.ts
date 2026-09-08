import { SetMetadata } from '@nestjs/common';
import { Rol } from '../../generated/prisma/enums';

export const ROLES_KEY = 'roles';

/**
 * Declara que roles pueden entrar a esta ruta. Requiere que el AuthGuard
 * ya haya corrido antes (para que exista request.user.rol) y que el
 * RolesGuard este aplicado en la misma ruta.
 * Uso: @Roles('ADMIN')
 */
export const Roles = (...roles: Rol[]) => SetMetadata(ROLES_KEY, roles);
