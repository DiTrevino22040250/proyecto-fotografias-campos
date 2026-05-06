import { Module, NestModule, MiddlewareConsumer } from '@nestjs/common';
import { TypeOrmModule } from '@nestjs/typeorm';
import { ConfigModule, ConfigService } from '@nestjs/config';
import { JwtModule } from '@nestjs/jwt';
import { MailerModule } from '@nestjs-modules/mailer';

// Entidades (Domain)
import { Pedido } from './domain/entities/pedido.entity';
import { Usuario } from './domain/entities/usuario.entity';

// Puertos (Domain/Ports)
import { PEDIDO_SERVICE_PORT } from './domain/ports/in/pedido-service.port';
import { PEDIDO_REPOSITORY_PORT } from './domain/ports/out/pedido-repository.port';
import { EMAIL_PORT } from './domain/ports/out/email.port';

// Servicios (Application)
import { PedidoService } from './application/services/pedido.service';
import { AuthService } from './application/services/auth.service';

// Adaptadores (Infrastructure)
import { PedidoRepository } from './infrastructure/adapters/out/persistence/pedido.repository';
import { EmailAdapter } from './infrastructure/adapters/out/email/email.adapter';
import { PedidoController } from './infrastructure/adapters/in/pedido.controller';
import { AuthController } from './infrastructure/adapters/in/auth.controller';

// Seguridad (Infrastructure)
import { JwtService } from './infrastructure/security/jwt.service';
import { JwtStrategy } from './infrastructure/security/jwt.strategy';
import { RateLimitMiddleware } from './infrastructure/middleware/rate-limit.middleware';
import { SanitizationMiddleware } from './infrastructure/middleware/sanitization.middleware';

@Module({
  imports: [
    // Carga de variables de entorno (.env)
    ConfigModule.forRoot({ isGlobal: true }),

    // Conexión a Base de Datos MySQL (XAMPP)
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
        synchronize: true, // Crea tablas automáticamente
      }),
    }),
    TypeOrmModule.forFeature([Pedido, Usuario]),

    // Configuración de JWT
    JwtModule.registerAsync({
      imports: [ConfigModule],
      inject: [ConfigService],
      useFactory: (config: ConfigService) => ({
        secret: config.get<string>('JWT_SECRET'),
        signOptions: { expiresIn: '1h' },
      }),
    }),

    // Configuración de Gmail
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
  controllers: [AuthController, PedidoController],
  providers: [
    AuthService,
    JwtService,
    JwtStrategy,
    // Inyección de dependencias mapeando Puertos a Clases
    { provide: PEDIDO_SERVICE_PORT, useClass: PedidoService },
    { provide: PEDIDO_REPOSITORY_PORT, useClass: PedidoRepository },
    { provide: EMAIL_PORT, useClass: EmailAdapter },
  ],
})
export class AppModule implements NestModule {
  // Aplicar Middlewares de Seguridad Globalmente
  configure(consumer: MiddlewareConsumer) {
    consumer
      .apply(RateLimitMiddleware, SanitizationMiddleware)
      .forRoutes('*');
  }
}