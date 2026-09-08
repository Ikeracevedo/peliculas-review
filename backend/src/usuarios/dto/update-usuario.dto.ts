import { IsEmail, IsEnum, IsOptional, IsString, MaxLength, MinLength } from 'class-validator';
import { Rol } from '../../generated/prisma/enums';

/**
 * DTO para que un ADMIN edite un usuario.
 * NO incluye `password` a propósito: resetear contraseñas es un flujo
 * de seguridad aparte (con su propio token de un solo uso), no un PATCH
 * cualquiera. Meterlo aquí sería mezclar dos responsabilidades distintas.
 */
export class UpdateUsuarioDto {
  @IsOptional()
  @IsString()
  @MinLength(1)
  @MaxLength(100)
  nombre?: string;

  @IsOptional()
  @IsEmail()
  correo?: string;

  @IsOptional()
  @IsEnum(Rol, { message: 'rol debe ser USUARIO o ADMIN' })
  rol?: Rol;
}
