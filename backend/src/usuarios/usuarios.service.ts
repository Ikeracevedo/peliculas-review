import { Injectable, NotFoundException } from '@nestjs/common';
import { PrismaService } from '../prisma/prisma.service';
import { UpdateUsuarioDto } from './dto/update-usuario.dto';

// Forma pública de un usuario: exactamente lo que es seguro devolver.
// `password` nunca aparece aquí, ni por accidente.
const SELECT_PUBLICO = {
  id: true,
  correo: true,
  nombre: true,
  rol: true,
  creadoEn: true,
  actualizadoEn: true,
} as const;

@Injectable()
export class UsuariosService {
  constructor(private readonly prisma: PrismaService) {}

  // --- Usados internamente por AuthService (necesitan el hash del password) ---

  crear(datos: { correo: string; nombre: string; password: string }) {
    // `password` ya debe llegar hasheada — AuthService es responsable de eso.
    return this.prisma.usuario.create({ data: datos });
  }

  buscarPorCorreo(correo: string) {
    return this.prisma.usuario.findUnique({ where: { correo } });
  }

  // --- CRUD de administración: nunca exponen el password ---

  findAll() {
    return this.prisma.usuario.findMany({
      select: SELECT_PUBLICO,
      orderBy: { creadoEn: 'desc' },
    });
  }

  async findOne(id: number) {
    const usuario = await this.prisma.usuario.findUnique({
      where: { id },
      select: SELECT_PUBLICO,
    });

    if (!usuario) throw new NotFoundException(`No existe un usuario con id ${id}`);
    return usuario;
  }

  async update(id: number, dto: UpdateUsuarioDto) {
    await this.findOne(id);
    return this.prisma.usuario.update({
      where: { id },
      data: dto,
      select: SELECT_PUBLICO,
    });
  }

  async remove(id: number) {
    await this.findOne(id);
    return this.prisma.usuario.delete({
      where: { id },
      select: SELECT_PUBLICO,
    });
  }
}
