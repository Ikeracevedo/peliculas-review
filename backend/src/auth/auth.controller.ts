import { Body, Controller, HttpCode, HttpStatus, Post } from '@nestjs/common';
import { AuthService } from './auth.service';
import { RegisterDto } from './dto/register.dto';
import { LoginDto } from './dto/login.dto';
import { Public } from './decorators/public.decorator';

@Controller('auth')
export class AuthController {
  constructor(private readonly authService: AuthService) {}

  @Public()
  @Post('registro')
  registrar(@Body() dto: RegisterDto) {
    return this.authService.registrar(dto);
  }

  @Public()
  @HttpCode(HttpStatus.OK) // POST por defecto responde 201; login es 200 (no crea nada)
  @Post('login')
  iniciarSesion(@Body() dto: LoginDto) {
    return this.authService.iniciarSesion(dto);
  }
}
