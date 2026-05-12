import { Controller, Get, Post, UseGuards } from '@nestjs/common';
import { EmailCronService } from '../../tasks/email-cron.service';
import { JwtAuthGuard } from '../../guards/jwt-auth.guard';

@Controller('cron')
@UseGuards(JwtAuthGuard)
export class CronController {
  constructor(private readonly cronService: EmailCronService) {}

  @Get('estado')
  obtenerEstado() {
    return this.cronService.getCronEstado();
  }

  @Post('reactivar')
  reactivar() {
    this.cronService.reactivarCron();
    return { mensaje: 'Cron reactivado correctamente' };
  }
}



