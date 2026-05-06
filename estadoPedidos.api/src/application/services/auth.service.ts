import { Injectable, ConflictException, UnauthorizedException } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import * as bcrypt from 'bcrypt';
import { Usuario, RolUsuario } from '../../domain/entities/usuario.entity'; // <-- Se usa en el constructor y en el rol
import { JwtService } from '../../infrastructure/security/jwt.service';
import { LoginDto } from '../dtos/login.dto';

@Injectable()
export class AuthService {
  constructor(
    @InjectRepository(Usuario) // <-- Aquí se usa 'Usuario'
    private readonly usuarioRepo: Repository<Usuario>,
    private readonly jwtService: JwtService,
  ) {}

  async registrar(data: any): Promise<Usuario> { // <-- Aquí se usa 'Usuario'
    const existe = await this.usuarioRepo.findOne({ where: { username: data.username } });
    if (existe) throw new ConflictException('El nombre de usuario ya existe');

    const salt = await bcrypt.genSalt(11);
    const passwordHash = await bcrypt.hash(data.password, salt); // <-- Generada

    const nuevoUsuario = this.usuarioRepo.create({
      username: data.username,
      email: data.email,
      nombreCompleto: data.nombreCompleto,
      passwordHash: passwordHash, // <-- ¡USADA AQUÍ!
      rol: data.rol || RolUsuario.CLIENTE, // <-- 'RolUsuario' USADO AQUÍ
    });

    return await this.usuarioRepo.save(nuevoUsuario);
  }

  async login(loginDto: LoginDto) {
    const usuario = await this.usuarioRepo.findOne({ where: { username: loginDto.username } });
    if (!usuario) throw new UnauthorizedException('Credenciales inválidas');

    const esValida = await bcrypt.compare(loginDto.password, usuario.passwordHash);
    if (!esValida) throw new UnauthorizedException('Credenciales inválidas');

    const token = this.jwtService.generarToken(usuario);
    return {
      access_token: token,
      user: {
        username: usuario.username,
        rol: usuario.rol,
      },
    };
  }
}