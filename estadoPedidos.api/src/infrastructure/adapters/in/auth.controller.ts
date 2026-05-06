import { Controller, Post, Body, UnauthorizedException, HttpCode, HttpStatus } from '@nestjs/common';
import { AuthService } from '../../../application/services/auth.service';
import { LoginDto } from '../../../application/dtos/login.dto';

@Controller('auth')
export class AuthController {
  constructor(private readonly authService: AuthService) {}

  @Post('register')
  async registrar(@Body() body: any) {
    return await this.authService.registrar(body);
  }

  @Post('login')
  @HttpCode(HttpStatus.OK)
  async login(@Body() loginDto: LoginDto) {
    const result = await this.authService.login(loginDto);
    if (!result) throw new UnauthorizedException('Credenciales incorrectas');
    return result;
  }
}


