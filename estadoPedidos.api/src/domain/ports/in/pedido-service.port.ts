import { EstadoPedido, Pedido } from '../../entities/pedido.entity';

export interface PedidoServicePort {
  obtenerTodos(username: string, rol: string): Promise<Pedido[]>;
  obtenerPorId(id: number, username: string, rol: string): Promise<Pedido>;
  actualizarEstado(id: number, nuevoEstado: EstadoPedido, user: any): Promise<Pedido>;
  cancelarPedido(id: number, user: any): Promise<Pedido>;
}

// Token para la inyección de dependencias en NestJS
export const PEDIDO_SERVICE_PORT = 'PEDIDO_SERVICE_PORT';


