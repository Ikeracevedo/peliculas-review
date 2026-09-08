import { ForbiddenException, Injectable, NotFoundException } from '@nestjs/common';
import { PrismaService } from '../prisma/prisma.service';
import { CreateReviewDto } from './dto/create-review.dto';
import { UpdateReviewDto } from './dto/update-review.dto';
import type { PayloadJwt } from '../auth/decorators/current-user.decorator';

@Injectable()
export class ReviewsService {
  constructor(private readonly prisma: PrismaService) {}

  create(dto: CreateReviewDto, autorId: number) {
    return this.prisma.review.create({ data: { ...dto, autorId } });
  }

  findAll() {
    return this.prisma.review.findMany({
      orderBy: { creadoEn: 'desc' },
      include: { pelicula: true, autor: { select: { id: true, nombre: true } } },
    });
  }

  async findOne(id: number) {
    const resena = await this.prisma.review.findUnique({
      where: { id },
      include: { pelicula: true, autor: { select: { id: true, nombre: true } } },
    });

    if (!resena) throw new NotFoundException(`No existe una reseña con id ${id}`);
    return resena;
  }

  async update(id: number, dto: UpdateReviewDto, usuarioActual: PayloadJwt) {
    const resena = await this.findOne(id);
    this.verificarPropietario(resena.autorId, usuarioActual);
    return this.prisma.review.update({ where: { id }, data: dto });
  }

  async remove(id: number, usuarioActual: PayloadJwt) {
    const resena = await this.findOne(id);
    this.verificarPropietario(resena.autorId, usuarioActual);
    return this.prisma.review.delete({ where: { id } });
  }

  // Un ADMIN puede moderar cualquier reseña. Un USUARIO normal solo la suya.
  private verificarPropietario(autorId: number, usuarioActual: PayloadJwt) {
    const esDueno = autorId === usuarioActual.sub;
    const esAdmin = usuarioActual.rol === 'ADMIN';

    if (!esDueno && !esAdmin) {
      throw new ForbiddenException('Solo puedes editar o eliminar tus propias reseñas');
    }
  }
}
