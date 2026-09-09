export interface Usuario {
  id: number;
  nombre: string;
  correo: string;
  rol: 'USUARIO' | 'ADMIN';
  creadoEn: string;
}

export interface Pelicula {
  id: number;
  nombre: string;
  imagen: string;
  descripcion: string | null;
  estreno: number | null;
  creadaEn: string;
  actualizadoEn: string;
}

export interface Review {
  id: number;
  titulo: string;
  contenido: string;
  calificacion: number;
  creadoEn: string;
  actualizadoEn: string;
  peliculaId: number;
  autorId: number;
  autor: { id: number; nombre: string };
  pelicula?: Pelicula;
}
