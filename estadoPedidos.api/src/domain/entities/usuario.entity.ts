import { Entity, Column, PrimaryGeneratedColumn } from 'typeorm';

export enum RolUsuario {
  ADMIN = 'admin',
  CLIENTE = 'cliente',
}

@Entity('usuarios')
export class Usuario {
  @PrimaryGeneratedColumn()
  id: number;

  @Column({ unique: true })
  username: string;

  @Column()
  passwordHash: string;

  @Column({ type: 'enum', enum: RolUsuario, default: RolUsuario.CLIENTE })
  rol: RolUsuario;

  @Column()
  email: string;

  @Column()
  nombreCompleto: string;
}
 
