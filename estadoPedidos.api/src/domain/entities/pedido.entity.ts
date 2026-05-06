import { Entity, Column, PrimaryGeneratedColumn, CreateDateColumn } from 'typeorm';

export enum EstadoPedido {
  EN_CURSO = 'en_curso',
  CANCELADO = 'cancelado',
  LISTO_PARA_RECOGIDA = 'listo_para_recogida',
}

@Entity('pedidos')
export class Pedido {
  @PrimaryGeneratedColumn()
  id: number;

  @Column()
  pedidoExternoId: number; // ID que viene de tu otra API (.NET)

  @Column()
  nombreCliente: string;

  @Column()
  emailCliente: string;

  @Column()
  tipoServicio: string;

  @Column({ type: 'enum', enum: EstadoPedido, default: EstadoPedido.EN_CURSO })
  estado: EstadoPedido;

  @Column()
  usuarioUsername: string; // Dueño del pedido

  @CreateDateColumn()
  fechaCreacion: Date;
}
 
