import { Injectable } from '@nestjs/common';
import { JwtService as NestJwtService } from '@nestjs/jwt';
import { Usuario } from '../../domain/entities/usuario.entity';

@Injectable()
export class JwtService {
  constructor(private readonly jwtService: NestJwtService) {}

  generarToken(usuario: Usuario): string {
    const payload = { 
      username: usuario.username, 
      rol: usuario.rol, 
      sub: usuario.id 
    };
    return this.jwtService.sign(payload);
  }
}
 
