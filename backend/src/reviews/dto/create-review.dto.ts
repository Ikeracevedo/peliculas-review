import { IsInt, IsString, Max, MaxLength, Min, MinLength } from 'class-validator';

export class CreateReviewDto {
  @IsString()
  @MinLength(1)
  @MaxLength(150)
  titulo: string;

  @IsString()
  @MinLength(1)
  @MaxLength(3000)
  contenido: string;

  @IsInt()
  @Min(1)
  @Max(5)
  calificacion: number;

  @IsInt()
  peliculaId: number;

  // autorId YA NO va aqui. Sale de request.user.sub (el JWT), puesto por
  // el AuthGuard. Ver ReviewsController.create y CurrentUser decorator.
}
