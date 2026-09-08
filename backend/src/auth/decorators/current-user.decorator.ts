import { createParamDecorator, ExecutionContext } from '@nestjs/common';

export interface PayloadJwt {
  sub: number; // id del usuario, convencion estandar del claim "sub" en JWT
  correo: string;
  rol: string;
}

/**
 * Extrae el usuario autenticado (puesto en request.user por el AuthGuard)
 * directo como argumento del metodo del controller.
 * Uso: create(@CurrentUser() usuario: PayloadJwt)
 */
export const CurrentUser = createParamDecorator(
  (_data: unknown, ctx: ExecutionContext): PayloadJwt => {
    const request = ctx.switchToHttp().getRequest();
    return request.user;
  },
);
