import { LoginDto } from '../../../application/dtos/login.dto';
import { Usuario } from '../../entities/usuario.entity';

export interface AuthServicePort {
  login(loginDto: LoginDto): Promise<{ access_token: string, user: any } | null>;
  registrar(data: any): Promise<Usuario>;
}

export const AUTH_SERVICE_PORT = 'AUTH_SERVICE_PORT';
 
//Tengo que aplicar el test sobre la interfaz no sobre authservice.port 