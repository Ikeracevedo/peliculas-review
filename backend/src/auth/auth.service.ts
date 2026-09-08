import { ConflictException, Injectable, UnauthorizedException } from '@nestjs/common';
import { JwtService } from '@nestjs/jwt';
import * as bcrypt from 'bcrypt';
import { UsuariosService } from '../usuarios/usuarios.service';
import { RegisterDto } from './dto/register.dto';
import { LoginDto } from './dto/login.dto';

// Costo del hash: cada +1 DUPLICA el tiempo de computo. 10 es el estandar
// razonable (~100ms) que balancea seguridad contra un login usable.
const SALT_ROUNDS = 10;

@Injectable()
export class AuthService {
  constructor(
    private readonly usuariosService: UsuariosService,
    private readonly jwtService: JwtService,
  ) {}

  async registrar(dto: RegisterDto) {
    const existente = await this.usuariosService.buscarPorCorreo(dto.correo);
    if (existente) {
      throw new ConflictException('Ya existe una cuenta con ese correo');
    }

    const passwordHasheada = await bcrypt.hash(dto.password, SALT_ROUNDS);

    // NOTA: nunca se lee dto.rol porque RegisterDto no lo declara.
    // Todo registro publico crea un usuario con rol USUARIO (default del schema).
    const usuario = await this.usuariosService.crear({
      correo: dto.correo,
      nombre: dto.nombre,
      password: passwordHasheada,
    });

    return this.firmarToken(usuario);
  }

  async iniciarSesion(dto: LoginDto) {
    const usuario = await this.usuariosService.buscarPorCorreo(dto.correo);

    // Mensaje generico a proposito: no decimos "correo no existe" vs
    // "contrasena incorrecta". Distinguirlos le regala a un atacante
    // informacion sobre que correos estan registrados (enumeracion de usuarios).
    if (!usuario) {
      throw new UnauthorizedException('Credenciales invalidas');
    }

    const passwordValida = await bcrypt.compare(dto.password, usuario.password);
    if (!passwordValida) {
      throw new UnauthorizedException('Credenciales invalidas');
    }

    return this.firmarToken(usuario);
  }

  private async firmarToken(usuario: { id: number; correo: string; nombre: string; rol: string }) {
    const payload = { sub: usuario.id, correo: usuario.correo, rol: usuario.rol };

    return {
      usuario: {
        id: usuario.id,
        correo: usuario.correo,
        nombre: usuario.nombre,
        rol: usuario.rol,
      },
      access_token: await this.jwtService.signAsync(payload),
    };
  }
}
