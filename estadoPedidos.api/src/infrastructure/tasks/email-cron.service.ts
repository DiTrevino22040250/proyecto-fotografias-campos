import { Injectable, Logger } from '@nestjs/common';
import { Cron } from '@nestjs/schedule';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { Pedido, EstadoPedido } from '../../domain/entities/pedido.entity';
import { EmailAdapter } from '../adapters/out/email/email.adapter';

// Patrones maliciosos que el cron debe detectar
const PATRONES_MALICIOSOS = [
  '<script', 'DROP TABLE', "' OR '1'='1",
  'javascript:', '../', '__proto__',
];

@Injectable()
export class EmailCronService {
  private readonly logger = new Logger(EmailCronService.name);
  private cronActivo = true;
  private readonly pedidosNotificados = new Set<number>();

  constructor(
    @InjectRepository(Pedido)
    private readonly pedidoRepo: Repository<Pedido>,
    private readonly emailAdapter: EmailAdapter,
  ) {}

  // Ejecuta cada 30 minutos
  @Cron('0 */30 * * * *')
  async verificarPedidosListos() {
    // Si el cron fue desactivado por vulneración, no ejecutar
    if (!this.cronActivo) {
      this.logger.warn('⛔ CRON DESACTIVADO: Fue vulnerado previamente. Requiere intervención manual.');
      return;
    }

    this.logger.log('🔄 Cron ejecutándose: Verificando pedidos listos para recogida...');

    try {
      const pedidosListos = await this.pedidoRepo.find({
        where: { estado: EstadoPedido.LISTO_PARA_RECOGIDA },
      });

      for (const pedido of pedidosListos) {
        // Verificar si el pedido contiene datos sensibles maliciosos
        if (this.detectarVulneracion(pedido)) {
          this.logger.error(`🚨 VULNERACIÓN DETECTADA en pedido ID: ${pedido.id}. CRON DESACTIVADO.`);
          this.cronActivo = false;
          return;
        }

        // Solo notificar si no fue notificado antes
        if (!this.pedidosNotificados.has(pedido.id)) {
          try {
            await this.emailAdapter.enviarCorreoListo(
              pedido.emailCliente,
              pedido.nombreCliente,
              pedido.tipoServicio,
            );
            this.pedidosNotificados.add(pedido.id);
            this.logger.log(`✅ Correo enviado a ${pedido.emailCliente} para pedido ID: ${pedido.id}`);
          } catch (error: any) {
            // Corrección: Casting a 'any' para acceder a .message
            this.logger.error(`❌ Error enviando correo para pedido ID: ${pedido.id} - ${error.message}`);
          }
        }
      }

      this.logger.log(`✅ Cron completado. Pedidos procesados: ${pedidosListos.length}`);
    } catch (error: any) {
      // Corrección: Casting a 'any' para acceder a .message
      this.logger.error(`❌ Error en cron: ${error.message}`);
    }
  }

  private detectarVulneracion(pedido: Pedido): boolean {
    const valores = [
      pedido.nombreCliente,
      pedido.emailCliente,
      pedido.tipoServicio,
      pedido.usuarioUsername,
    ];

    for (const valor of valores) {
      if (!valor) continue;
      const valorUpper = valor.toUpperCase();
      for (const patron of PATRONES_MALICIOSOS) {
        if (valorUpper.includes(patron.toUpperCase())) {
          this.logger.error(`🚨 Dato sensible detectado: "${patron}" en campo del pedido ID: ${pedido.id}`);
          return true;
        }
      }
    }
    return false;
  }

  // Método para reactivar el cron manualmente (solo admin)
  reactivarCron(): void {
    this.cronActivo = true;
    this.logger.log('✅ Cron reactivado manualmente por administrador');
  }

  getCronEstado(): { activo: boolean } {
    return { activo: this.cronActivo };
  }
}