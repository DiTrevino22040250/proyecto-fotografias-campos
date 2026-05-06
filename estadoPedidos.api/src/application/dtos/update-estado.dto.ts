import { IsEnum, IsNotEmpty } from 'class-validator';
import { EstadoPedido } from '../../domain/entities/pedido.entity';

export class UpdateEstadoDto {
  @IsNotEmpty()
  @IsEnum(EstadoPedido, {
    message: 'El estado debe ser: en_curso, cancelado o listo_para_recogida',
  })
  estado: EstadoPedido;
}
 
