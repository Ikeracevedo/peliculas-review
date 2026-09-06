import { Injectable, NotFoundException } from '@nestjs/common';
import { PrismaService } from '../prisma/prisma.service';
import { CreatePeliculaDto } from './dto/create-pelicula.dto';
import { UpdatePeliculaDto } from './dto/update-pelicula.dto';

@Injectable()
export class PeliculasService {

  constructor(private readonly prisma: PrismaService) {}

  create(dto: CreatePeliculaDto) {
    return this.prisma.pelicula.create({ data: dto });
  }

  findAll() {
    return this.prisma.pelicula.findMany({
      orderBy: { creadaEn: 'desc'},
    });
  }

  async findOne(id: number) {
    const pelicula = await this.prisma.pelicula.findUnique({ where: { id}});

    if (!pelicula) {
      throw new NotFoundException('No existe una pelicula con ese id')
    }

    return pelicula;

  }

  async update(id: number, dto: UpdatePeliculaDto) {
    await this.findOne(id);     // Lanza 404 sino existe, antes de hacer el update
    return this.prisma.pelicula.update({ where: { id }, data: dto});
  }

  async remove(id: number) {
    await this.findOne(id);
    return this.prisma.pelicula.delete({ where: { id } });
  }
}
