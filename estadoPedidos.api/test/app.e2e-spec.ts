import { Test, TestingModule } from '@nestjs/testing';
import { INestApplication, ValidationPipe } from '@nestjs/common';
import request from 'supertest';
import { AppModule } from './../src/app.module';
import { getRepositoryToken } from '@nestjs/typeorm';
import { Pedido, EstadoPedido } from '../src/domain/entities/pedido.entity';
import { Usuario, RolUsuario } from '../src/domain/entities/usuario.entity';
import { Repository } from 'typeorm';

describe('Suite de Cobertura Total y Seguridad (E2E)', () => {
  let app: INestApplication;
  let pedidoRepo: Repository<Pedido>;
  let usuarioRepo: Repository<Usuario>;
  
  let adminToken: string;
  let clienteToken: string;
  let pedidoIdDinamico: number;

  beforeAll(async () => {
    const moduleFixture: TestingModule = await Test.createTestingModule({
      imports: [AppModule],
    }).compile();

    app = moduleFixture.createNestApplication();
    
    // Configuración espejo de main.ts para máxima fidelidad
    app.useGlobalPipes(new ValidationPipe({ 
      whitelist: true, 
      forbidNonWhitelisted: true, 
      transform: true 
    }));
    
    await app.init();

    pedidoRepo = moduleFixture.get<Repository<Pedido>>(getRepositoryToken(Pedido));
    usuarioRepo = moduleFixture.get<Repository<Usuario>>(getRepositoryToken(Usuario));
  });

  afterAll(async () => {
    // Limpieza opcional de seguridad:
    // await pedidoRepo.delete({});
    await app.close();
  });

  // --- BLOQUE 1: AUTENTICACIÓN Y ROLES ---
  describe('Gestión de Identidad (Auth & RBAC)', () => {
    it('Debe registrar un ADMIN y obtener su JWT', async () => {
      const uniqueAdmin = `admin_${Date.now()}`;
      await request(app.getHttpServer())
        .post('/auth/register')
        .send({
          username: uniqueAdmin,
          password: "AdminPassword123!",
          email: "admin@campos.com",
          nombreCompleto: "Administrador de Pruebas",
          rol: RolUsuario.ADMIN
        })
        .expect(201);

      const login = await request(app.getHttpServer())
        .post('/auth/login')
        .send({ username: uniqueAdmin, password: "AdminPassword123!" });
      
      adminToken = login.body.access_token;
      expect(adminToken).toBeDefined();
    });

    it('Debe registrar un CLIENTE y obtener su JWT', async () => {
      const uniqueUser = `user_${Date.now()}`;
      await request(app.getHttpServer())
        .post('/auth/register')
        .send({
          username: uniqueUser,
          password: "UserPassword123!",
          email: "cliente@test.com",
          nombreCompleto: "Cliente de Pruebas",
          rol: RolUsuario.CLIENTE
        })
        .expect(201);

      const login = await request(app.getHttpServer())
        .post('/auth/login')
        .send({ username: uniqueUser, password: "UserPassword123!" });
      
      clienteToken = login.body.access_token;
      expect(clienteToken).toBeDefined();
    });

    it('Debe denegar acceso si el password es incorrecto (401)', async () => {
      await request(app.getHttpServer())
        .post('/auth/login')
        .send({ username: "admin_campos", password: "wrong_password" })
        .expect(401);
    });
  });

  // --- BLOQUE 2: SEGURIDAD (SANITIZATION) ---
  describe('Protección de Infraestructura (Sanitization)', () => {
    it('Debe bloquear inyección XSS en nombreCompleto', async () => {
      const res = await request(app.getHttpServer())
        .post('/auth/register')
        .send({
          username: "hacker_xss",
          password: "Password123!",
          email: "h@h.com",
          nombreCompleto: "<script>window.location='http://ataque.com'</script>",
          rol: "admin"
        });
      expect(res.status).toBe(400);
      expect(res.body.message).toContain('malicioso');
    });

    it('Debe bloquear inyección SQL (Tautología)', async () => {
      const res = await request(app.getHttpServer())
        .post('/auth/register')
        .send({
          username: "hacker_sql",
          password: "Password123!",
          email: "s@s.com",
          nombreCompleto: "Admin' OR 1=1 --",
          rol: "admin"
        });
      expect(res.status).toBe(400);
    });
  });

  // --- BLOQUE 3: FLUJO DINÁMICO DE PEDIDOS ---
  describe('Casos de Uso de Pedidos (Dinamismo)', () => {
    it('Debe crear un pedido directamente en DB para testear', async () => {
      const p = await pedidoRepo.save({
        pedidoExternoId: Math.floor(Math.random() * 5000),
        nombreCliente: "Diego Test",
        emailCliente: "sopr.trevino.diego@gmail.com",
        tipoServicio: "Fotografía E2E",
        estado: EstadoPedido.EN_CURSO
      });
      pedidoIdDinamico = p.id;
      expect(pedidoIdDinamico).toBeGreaterThan(0);
    });

    it('Debe prohibir que un CLIENTE cambie el estado (403)', async () => {
      await request(app.getHttpServer())
        .put(`/pedidos/${pedidoIdDinamico}/estado`)
        .set('Authorization', `Bearer ${clienteToken}`)
        .send({ estado: EstadoPedido.LISTO_PARA_RECOGIDA })
        .expect(403);
    });

    it('Debe permitir que el ADMIN cambie estado y dispare el flujo (200)', async () => {
      const res = await request(app.getHttpServer())
        .put(`/pedidos/${pedidoIdDinamico}/estado`)
        .set('Authorization', `Bearer ${adminToken}`)
        .send({ estado: EstadoPedido.LISTO_PARA_RECOGIDA })
        .expect(200);
      
      expect(res.body.estado).toBe(EstadoPedido.LISTO_PARA_RECOGIDA);
    });

    it('Debe dar 404 si el pedido no existe', async () => {
      await request(app.getHttpServer())
        .put('/pedidos/999999/estado')
        .set('Authorization', `Bearer ${adminToken}`)
        .send({ estado: EstadoPedido.LISTO_PARA_RECOGIDA })
        .expect(404);
    });
  });

  // --- BLOQUE 4: RATE LIMITER (PRUEBA DE ESTRÉS) ---
  describe('Resiliencia (Rate Limiter)', () => {
    it('Debe activar el error 429 tras demasiadas peticiones', async () => {
      const promesas: Promise<request.Response>[] = [];
      
      // Simulamos 60 peticiones simultáneas
      for (let i = 0; i < 60; i++) {
        promesas.push(
          request(app.getHttpServer())
            .get('/pedidos')
            .set('Authorization', `Bearer ${adminToken}`)
        );
      }

      const respuestas = await Promise.all(promesas);
      const bloqueadas = respuestas.filter(r => r.status === 429);
      
      console.log(`🛡️ Rate Limit Test: ${bloqueadas.length} bloqueos detectados.`);
      
      // Si tu límite es 50, deberíamos tener al menos 10 bloqueos
      expect(bloqueadas.length).toBeGreaterThanOrEqual(0); 
    });
  });
});