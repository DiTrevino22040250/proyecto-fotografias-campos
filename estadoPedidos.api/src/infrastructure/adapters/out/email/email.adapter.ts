import { Injectable } from '@nestjs/common';
import { MailerService } from '@nestjs-modules/mailer';
import { type EmailPort } from '../../../../domain/ports/out/email.port';

@Injectable()
export class EmailAdapter implements EmailPort {
  constructor(private readonly mailerService: MailerService) {}

  async enviarCorreoListo(email: string, nombre: string, servicio: string): Promise<void> {
    await this.mailerService.sendMail({
      to: email,
      subject: '¡Tu pedido está listo para recoger! 📸',
      html: `
        <div style="font-family: Arial, sans-serif; border: 1px solid #ddd; padding: 20px;">
          <h2 style="color: #2c3e50;">¡Hola ${nombre}!</h2>
          <p>Tenemos excelentes noticias: tu pedido de <b>${servicio}</b> ya está terminado.</p>
          <p>Puedes pasar a recogerlo en nuestro estudio en el horario habitual.</p>
          <hr>
          <p style="font-size: 0.9em; color: #7f8c8d;">Gracias por confiar en Fotografías Campos.</p>
        </div>
      `,
    });
  }
} 
