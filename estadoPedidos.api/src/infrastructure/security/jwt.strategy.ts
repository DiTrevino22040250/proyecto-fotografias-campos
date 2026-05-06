import { Injectable, UnauthorizedException } from '@nestjs/common';
import { PassportStrategy } from '@nestjs/passport';
import { ExtractJwt, Strategy } from 'passport-jwt';
import { ConfigService } from '@nestjs/config';

@Injectable()
export class JwtStrategy extends PassportStrategy(Strategy) {
  constructor(private readonly configService: ConfigService) {
    const secret = configService.get<string>('JWT_SECRET');
    
    if (!secret) {
      throw new Error('JWT_SECRET no está definido en las variables de entorno');
    }

    super({
      jwtFromRequest: ExtractJwt.fromAuthHeaderAsBearerToken(),
      ignoreExpiration: false,
      secretOrKey: secret, // Pasamos la constante ya validada como string
    });
  }

  async validate(payload: any) {
    // Validamos que el payload tenga la estructura que esperamos
    if (!payload || !payload.username) {
      throw new UnauthorizedException('Token no contiene información válida');
    }
    
    return { 
      username: payload.username, 
      rol: payload.rol, 
      id: payload.sub 
    };
  }
}