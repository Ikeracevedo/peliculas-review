import { Injectable, NotFoundException } from '@nestjs/common';
import { PrismaService } from '../prisma/prisma.service';
import { CreatePeliculaDto } from './dto/create-pelicula.dto';
import { UpdatePeliculaDto } from './dto/update-pelicula.dto';

@Injectable()
export class PeliculasService {

  // Constructor para inyectar el servicio de Prisma, que nos permite interactuar con la base de datos
  constructor(private readonly prisma: PrismaService) {}

  /**
   * Crea una nueva película en la base de datos utilizando el marco de datos del DTO
   * @param dto - Objeto para crear una pelicula
   * @returns - una promesa que resuelve la película creada
   */
  create(dto: CreatePeliculaDto) {
    return this.prisma.pelicula.create({ data: dto });
  }

  /**
   * Busca entre toda la traba pelicula en DB y devuelve un arreglo en orden descendente por fecha
   * @returns - una promesa que resuelve un arreglo de películas
   */
  findAll() {
    return this.prisma.pelicula.findMany({
      orderBy: { creadaEn: 'desc'},
    });
  }

  /**
   * Recibe un id de pelicula y busca en base de datos si existe sino lanza un 404 que no fue encontrada el id
   * @param id - id de la pelicula a buscar
   * @returns - promesa con la pelicula encontrada o un error 404 si no existe
   */
  async findOne(id: number) {
    const pelicula = await this.prisma.pelicula.findUnique({ where: { id}});

    if (!pelicula) {
      throw new NotFoundException('No existe una pelicula con ese id')
    }

    return pelicula;

  }

  /**
   * Actrualiza una pelicula en la base de datos, primero verifica si existe sino lanza un 404
   * @param id - id de la pelicula a actualizar
   * @param dto - datos para actualizar la pelicula
   * @returns - promesa con la pelicula actualizada
   */
  async update(id: number, dto: UpdatePeliculaDto) {
    await this.findOne(id);     
    return this.prisma.pelicula.update({ where: { id }, data: dto});
  }

  /**
   * Elimina una peliocula de base de datos, valida que exista y si es asi la elimina sino lanza error
   * @param id - id de la pelicula a eliminar
   * @returns - promesa con la pelicula eliminadas
   */
  async remove(id: number) {
    await this.findOne(id);
    return this.prisma.pelicula.delete({ where: { id } });
  }
}
