import { Module, NestModule, MiddlewareConsumer } from '@nestjs/common';
import { TypeOrmModule } from '@nestjs/typeorm';
import { ConfigModule, ConfigService } from '@nestjs/config';
import { JwtModule } from '@nestjs/jwt';
import { MailerModule } from '@nestjs-modules/mailer';
import { ScheduleModule } from '@nestjs/schedule';

import { Pedido } from './domain/entities/pedido.entity';
import { Usuario } from './domain/entities/usuario.entity';

import { PEDIDO_SERVICE_PORT } from './domain/ports/in/pedido-service.port';
import { PEDIDO_REPOSITORY_PORT } from './domain/ports/out/pedido-repository.port';
import { EMAIL_PORT } from './domain/ports/out/email.port';

import { PedidoService } from './application/services/pedido.service';
import { AuthService } from './application/services/auth.service';

import { PedidoRepository } from './infrastructure/adapters/out/persistence/pedido.repository';
import { EmailAdapter } from './infrastructure/adapters/out/email/email.adapter';
import { PedidoController } from './infrastructure/adapters/in/pedido.controller';
import { AuthController } from './infrastructure/adapters/in/auth.controller';
import { CronController } from './infrastructure/adapters/in/cron.controller';

import { JwtService } from './infrastructure/security/jwt.service';
import { JwtStrategy } from './infrastructure/security/jwt.strategy';
import { RateLimitMiddleware } from './infrastructure/middleware/rate-limit.middleware';
import { SanitizationMiddleware } from './infrastructure/middleware/sanitization.middleware';
import { SecurityLoggingMiddleware } from './infrastructure/middleware/security-logging.middleware';
import { EmailCronService } from './infrastructure/tasks/email-cron.service';

@Module({
  imports: [
    ConfigModule.forRoot({ isGlobal: true }),
    ScheduleModule.forRoot(),

    TypeOrmModule.forRootAsync({
      imports: [ConfigModule],
      inject: [ConfigService],
      useFactory: (config: ConfigService) => ({
        type: 'mysql',
        host: config.get<string>('DB_HOST', 'localhost'),
        port: config.get<number>('DB_PORT', 3306),
        username: config.get<string>('DB_USER', 'root'),
        password: config.get<string>('DB_PASS', ''),
        database: config.get<string>('DB_NAME', 'estado_pedidos'),
        entities: [Pedido, Usuario],
        synchronize: true,
      }),
    }),
    TypeOrmModule.forFeature([Pedido, Usuario]),

    JwtModule.registerAsync({
      imports: [ConfigModule],
      inject: [ConfigService],
      useFactory: (config: ConfigService) => ({
        secret: config.get<string>('JWT_SECRET'),
        signOptions: { expiresIn: '1h' },
      }),
    }),

    MailerModule.forRootAsync({
      imports: [ConfigModule],
      inject: [ConfigService],
      useFactory: (config: ConfigService) => ({
        transport: {
          host: 'smtp.gmail.com',
          port: 587,
          secure: false,
          auth: {
            user: config.get<string>('EMAIL_USER'),
            pass: config.get<string>('EMAIL_PASS'),
          },
        },
      }),
    }),
  ],
  controllers: [AuthController, PedidoController, CronController],
  providers: [
    AuthService,
    JwtService,
    JwtStrategy,
    EmailCronService,
    // Registramos el repositorio para que PedidoRepository esté disponible por su clase
    PedidoRepository,
    // Registramos EmailAdapter directamente para que EmailCronService lo encuentre
    EmailAdapter, 
    // Mantenemos los ports para la arquitectura hexagonal
    { provide: PEDIDO_SERVICE_PORT, useClass: PedidoService },
    { provide: PEDIDO_REPOSITORY_PORT, useClass: PedidoRepository },
    { provide: EMAIL_PORT, useClass: EmailAdapter },
  ],
})
export class AppModule implements NestModule {
  configure(consumer: MiddlewareConsumer) {
    consumer
      .apply(RateLimitMiddleware, SanitizationMiddleware, SecurityLoggingMiddleware)
      .forRoutes('*');
  }
}