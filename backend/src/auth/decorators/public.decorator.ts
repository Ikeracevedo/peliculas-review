import { SetMetadata } from '@nestjs/common';

export const IS_PUBLIC_KEY = 'isPublic';

/**
 * Marca una ruta como accesible sin JWT. El AuthGuard global la deja pasar.
 * Uso: @Public() encima del metodo del controller.
 */
export const Public = () => SetMetadata(IS_PUBLIC_KEY, true);
