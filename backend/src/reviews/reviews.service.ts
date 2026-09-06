import { Injectable, NotFoundException } from '@nestjs/common';
import { PrismaService } from '../prisma/prisma.service';
import { CreateReviewDto } from './dto/create-review.dto';
import { UpdateReviewDto } from './dto/update-review.dto';

@Injectable()
export class ReviewsService {

  constructor(private readonly prisma: PrismaService) {}
  

  create(dto: CreateReviewDto) {
    return this.prisma.review.create({ data: dto });
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

  async update(id: number, dto: UpdateReviewDto) {
    await this.findOne(id);
    return this.prisma.review.update({ where: { id }, data: dto });
  }

  async remove(id: number) {
    await this.findOne(id);
    return this.prisma.review.delete({ where: { id } });
  }
}
