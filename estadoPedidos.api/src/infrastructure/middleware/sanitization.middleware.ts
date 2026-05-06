import { Injectable, NestMiddleware, BadRequestException } from '@nestjs/common';
import { Request, Response, NextFunction } from 'express';

@Injectable()
export class SanitizationMiddleware implements NestMiddleware {
  use(req: Request, res: Response, next: NextFunction) {
    if (req.body && Object.keys(req.body).length > 0) {
      const bodyString = JSON.stringify(req.body);

      const diccionarioMalicioso = {
        // XSS: Scripts, iframes y eventos de ejecución (onerror, onclick)
        xss: /<script\b[^>]*>([\s\S]*?)<\/script>|on\w+\s*=\s*["'][^"']*["']|<iframe\b[^>]*>|javascript:/gi,
        
        // SQL Injection: Comandos clásicos y tautologías (OR 1=1)
        sql: /--|#|\/\*|\*\/|(\b(SELECT|INSERT|UPDATE|DELETE|DROP|UNION|ALTER|TRUNCATE|EXEC|DECLARE|GRANT|REVOKE)\b)|(\bOR\b\s+['"]?\d+['"]?\s*=\s*['"]?\d+['"]?)/gi,
        
        // Path Traversal: Intentos de acceder a archivos del sistema
        path: /(\.\.\/|\.\.\\|etc\/passwd|cmd\.exe|\/bin\/sh)/gi,
        
        // NoSQL/Proto: Prevención de contaminación de prototipos o inyecciones NoSQL
        nosql: /\$(gt|lt|gte|lte|ne|eq|where|regex|proto|constructor)/gi
      };

      for (const [tipo, patron] of Object.entries(diccionarioMalicioso)) {
        if (patron.test(bodyString)) {
          console.warn(`[SEGURIDAD] Intento de ataque ${tipo.toUpperCase()} bloqueado desde IP: ${req.ip}`);
          throw new BadRequestException(`Petición rechazada por seguridad: Contenido malicioso detectado (${tipo}).`);
        }
      }
    }
    next();
  }
}