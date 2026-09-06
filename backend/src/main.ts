import { ValidationPipe } from '@nestjs/common';
import { NestFactory } from '@nestjs/core';
import { AppModule } from './app.module';

async function bootstrap() {
  const app = await NestFactory.create(AppModule);

  // Permite que el front consuma el backend y no le coloque problemas por CORS
  app.enableCors({
    origin: ['http://localhost:5173'], // Configuracion del origen de acuerdo a si esta en desarrollo o en prod
    credentials: true,
  });

  // Prefijo api
  app.setGlobalPrefix('api');

  app.useGlobalPipes(
    new ValidationPipe({
      whitelist: true,    // Borra las propiedades no declaradoas
      forbidNonWhitelisted: true,    // Responde con 400 si llegan
      transform: true,    // Convierte el JSON plano en instancia del DTO
    }),
  );



  await app.listen(process.env.PORT ?? 3000);
}
bootstrap();
