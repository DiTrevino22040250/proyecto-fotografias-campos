import { Controller, Get, Put, Body, Param, UseGuards, Req, ParseIntPipe, Inject } from '@nestjs/common';
import { type PedidoServicePort, PEDIDO_SERVICE_PORT } from '../../../domain/ports/in/pedido-service.port';
import { UpdateEstadoDto } from '../../../application/dtos/update-estado.dto';
import { JwtAuthGuard } from '../../guards/jwt-auth.guard';

@Controller('pedidos')
@UseGuards(JwtAuthGuard)
export class PedidoController {
  constructor(
    @Inject(PEDIDO_SERVICE_PORT) 
    private readonly pedidoService: PedidoServicePort
  ) {}

  @Get()
  async findAll(@Req() req) {
    return await this.pedidoService.obtenerTodos(req.user.username, req.user.rol);
  }

  @Put(':id/estado')
  async updateEstado(@Param('id', ParseIntPipe) id: number, @Body() dto: UpdateEstadoDto, @Req() req) {
    return await this.pedidoService.actualizarEstado(id, dto.estado, req.user);
  }

  @Put(':id/cancelar')
  async cancelar(@Param('id', ParseIntPipe) id: number, @Req() req) {
    return await this.pedidoService.cancelarPedido(id, req.user);
  }
}
 
