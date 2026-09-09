import {
    IsInt,
    IsOptional,
    IsString,
    IsUrl,
    Max,
    MaxLength,
    Min,
    MinLength,
  } from 'class-validator';
  
  export class CreatePeliculaDto {
    @IsString()
    @MinLength(1)
    @MaxLength(200)
    nombre: string;
  
    @IsUrl()
    imagen: string;
  
    @IsOptional()
    @IsString()
    @MaxLength(2000)
    sinopsis?: string;
  
    @IsOptional()
    @IsInt()
    @Min(1888) // año de la primera película que se conserva
    @Max(2100)
    estreno?: number;
  }