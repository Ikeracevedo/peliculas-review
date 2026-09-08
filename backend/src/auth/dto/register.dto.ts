import { IsEmail, IsString, MaxLength, MinLength } from 'class-validator';

export class RegisterDto {
  @IsEmail()
  correo: string;

  @IsString()
  @MinLength(2)
  @MaxLength(100)
  nombre: string;

  // 8 caracteres mínimo: el estándar razonable para un proyecto académico.
  // La fuerza real de la contraseña la da bcrypt, no la longitud sola.
  @IsString()
  @MinLength(8)
  @MaxLength(72) // bcrypt trunca e ignora cualquier caracter después de 72 bytes
  password: string;
}
