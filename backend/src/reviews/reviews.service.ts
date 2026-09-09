import { ForbiddenException, Injectable, NotFoundException } from '@nestjs/common';
import { PrismaService } from '../prisma/prisma.service';
import { CreateReviewDto } from './dto/create-review.dto';
import { UpdateReviewDto } from './dto/update-review.dto';
import type { PayloadJwt } from '../auth/decorators/current-user.decorator';

@Injectable()
export class ReviewsService {
  // El prisma lo crea directamente el servicio de Prisma en su clase y en la 
  // Clase reviewsService lo inyectamos o lo requerimos para que funcione
  constructor(private readonly prisma: PrismaService) {}

  /**
   *  Crea una nueva reseña en la base de datos utilizando el marco de datos del DTO
   * @param dto - Objeto para crear una reseña
   * @param autorId - id del usuario que crea la reseña
   * @returns - promesa de la creacion en base de datos
   */
  create(dto: CreateReviewDto, autorId: number) {
    return this.prisma.review.create({ data: { ...dto, autorId } });
  }

  /**
   *  Busca en base de datos en orden desencdente pro fecha de creacion
   *  Ademas trae las llaves foraneas de la tabla reseñas
   * @returns 
   */
  findAll() {
    return this.prisma.review.findMany({
      orderBy: { creadoEn: 'desc' },
      include: { pelicula: true, autor: { select: { id: true, nombre: true } } },
    });
  }

  /**
   * Busca una reseña de acuerdo a su id y trae todos los campso hasta los de la llave foranea
   * @param id - Identificacion de la reseña
   * @returns - promesa con la reseña
   */
  async findOne(id: number) {
    const resena = await this.prisma.review.findUnique({
      where: { id },
      include: { pelicula: true, autor: { select: { id: true, nombre: true } } },
    });

    if (!resena) throw new NotFoundException(`No existe una reseña con id ${id}`);
    return resena;
  }

  /**
   * Actualiza la reseña ya creada previamente
   * 
   * @param id - identificador de la reseña
   * @param dto - datos para actualizar la reseña solo los datos de la creacion no puede actualizar un ID
   * @param usuarioActual - Usuario que hace el update
   * @returns - promesa de actualziacion 
   */
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

  // Un admin puede moderar cualquier reseña. Un usuario normal solo la suya.
  private verificarPropietario(autorId: number, usuarioActual: PayloadJwt) {
    const esDueno = autorId === usuarioActual.sub;
    const esAdmin = usuarioActual.rol === 'ADMIN';

    if (!esDueno && !esAdmin) {
      throw new ForbiddenException('Solo puedes editar o eliminar tus propias reseñas');
    }
  }
}
