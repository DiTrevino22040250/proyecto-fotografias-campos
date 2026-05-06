import { Injectable } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { Pedido, EstadoPedido } from '../../../../domain/entities/pedido.entity';
import { type PedidoRepositoryPort } from '../../../../domain/ports/out/pedido-repository.port';

@Injectable()
export class PedidoRepository implements PedidoRepositoryPort {
  constructor(
    @InjectRepository(Pedido)
    private readonly repository: Repository<Pedido>,
  ) {}

  async findAll(): Promise<Pedido[]> {
    return await this.repository.find();
  }

  async findById(id: number): Promise<Pedido | null> {
    return await this.repository.findOne({ where: { id } });
  }

  async findByUsername(username: string): Promise<Pedido[]> {
    return await this.repository.find({ where: { usuarioUsername: username } });
  }

  async create(pedido: Partial<Pedido>): Promise<Pedido> {
    const nuevo = this.repository.create(pedido);
    return await this.repository.save(nuevo);
  }

  async updateEstado(id: number, estado: EstadoPedido): Promise<Pedido | null> {
    await this.repository.update(id, { estado });
    return await this.findById(id);
  }
}
 
