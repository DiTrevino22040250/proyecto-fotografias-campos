import { Injectable, Inject, ForbiddenException, NotFoundException } from '@nestjs/common';
import { EstadoPedido, Pedido } from '../../domain/entities/pedido.entity';
import { PEDIDO_REPOSITORY_PORT, type PedidoRepositoryPort } from '../../domain/ports/out/pedido-repository.port';
import { EMAIL_PORT, type EmailPort } from '../../domain/ports/out/email.port';
import { PedidoServicePort } from '../../domain/ports/in/pedido-service.port';

@Injectable()
export class PedidoService implements PedidoServicePort {
  constructor(
    @Inject(PEDIDO_REPOSITORY_PORT) 
    private readonly repo: PedidoRepositoryPort,
    @Inject(EMAIL_PORT) 
    private readonly emailAdapter: EmailPort,
  ) {}

  async crearPedido(data: any, user: any): Promise<Pedido> {
    return this.repo.create({
        pedidoExternoId: data.pedidoExternoId,
        nombreCliente: data.nombreCliente,
        emailCliente: data.emailCliente,
        tipoServicio: data.tipoServicio,
        usuarioUsername: data.usuarioUsername || user.username,
        estado: EstadoPedido.EN_CURSO
    });
}

  async obtenerTodos(username: string, rol: string): Promise<Pedido[]> {
    if (rol === 'admin') return this.repo.findAll();
    return this.repo.findByUsername(username);
  }

  async obtenerPorId(id: number, username: string, rol: string): Promise<Pedido> {
    const pedido = await this.repo.findById(id);
    if (!pedido) throw new NotFoundException('Pedido no encontrado');

    // Seguridad: Si no es admin, solo puede ver su propio pedido
    if (rol !== 'admin' && pedido.usuarioUsername !== username) {
      throw new ForbiddenException('No tienes permiso para ver este pedido');
    }
    return pedido;
  }

  async actualizarEstado(id: number, nuevoEstado: EstadoPedido, user: any): Promise<Pedido> {
    // Seguridad Crítica: Solo el Admin puede cambiar estados (especialmente a "Listo")
    if (user.rol !== 'admin') {
      throw new ForbiddenException('Acceso denegado. Solo administradores pueden realizar esta acción');
    }

    const pedido = await this.repo.findById(id);
    if (!pedido) throw new NotFoundException('Pedido no encontrado');

    const actualizado = await this.repo.updateEstado(id, nuevoEstado);
  
  if (!actualizado) {
    throw new NotFoundException(`No se pudo actualizar el pedido con ID ${id}`);
  }

  if (nuevoEstado === EstadoPedido.LISTO_PARA_RECOGIDA) {
    await this.emailAdapter.enviarCorreoListo(pedido.emailCliente, pedido.nombreCliente, pedido.tipoServicio);
  }

  return actualizado; // Ahora TypeScript sabe que aquí ya no es null

   
  }

  async cancelarPedido(id: number, user: any): Promise<Pedido> {
  const pedido = await this.repo.findById(id);
  
  if (!pedido) {
    throw new NotFoundException('Pedido no encontrado');
  }

  // El usuario puede cancelar su propio pedido, el admin cualquiera
  if (user.rol !== 'admin' && pedido.usuarioUsername !== user.username) {
    throw new ForbiddenException('No tienes permiso para cancelar este pedido');
  }

  // El usuario no puede cancelar si ya está listo para recogida (evita fraudes)
  if (pedido.estado === EstadoPedido.LISTO_PARA_RECOGIDA && user.rol !== 'admin') {
    throw new ForbiddenException('No puedes cancelar un pedido que ya está listo para entrega');
  }

  // Guardamos el resultado en una constante para verificar el tipo
  const pedidoCancelado = await this.repo.updateEstado(id, EstadoPedido.CANCELADO);

  // Si por alguna razón la DB no devuelve el objeto, lanzamos error
  if (!pedidoCancelado) {
    throw new NotFoundException('No se pudo procesar la cancelación en la base de datos');
  }

  // Ahora TypeScript está seguro de que NO es null y te dejará compilar
  return pedidoCancelado;
}
}
 
