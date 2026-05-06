import { Pedido, EstadoPedido } from '../../entities/pedido.entity';

export interface PedidoRepositoryPort {
  findAll(): Promise<Pedido[]>;
  findById(id: number): Promise<Pedido | null>;
  findByUsername(username: string): Promise<Pedido[]>;
  updateEstado(id: number, estado: EstadoPedido): Promise<Pedido | null>;
  create(pedido: Partial<Pedido>): Promise<Pedido>;
}
export const PEDIDO_REPOSITORY_PORT = 'PEDIDO_REPOSITORY_PORT'; 
