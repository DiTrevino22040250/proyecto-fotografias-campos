 import { Injectable, NestMiddleware, Logger } from '@nestjs/common';
import { Request, Response, NextFunction } from 'express';

@Injectable()
export class SecurityLoggingMiddleware implements NestMiddleware {
  private readonly logger = new Logger('SecurityLogger');

  use(req: Request, res: Response, next: NextFunction) {
    const ip = req.headers['x-test-ip'] as string || req.ip || 'unknown';
    const method = req.method;
    const path = req.path;

    this.logger.log(`📥 REQUEST | IP: ${ip} | ${method} ${path}`);

    res.on('finish', () => {
      const status = res.statusCode;
      if (status === 400)
        this.logger.warn(`⚠️ BAD REQUEST | IP: ${ip} | ${method} ${path} | Posible ataque`);
      if (status === 401)
        this.logger.warn(`🔒 UNAUTHORIZED | IP: ${ip} | ${method} ${path}`);
      if (status === 429)
        this.logger.error(`🚨 RATE LIMIT | IP: ${ip} | ${method} ${path} | Posible DDoS/fuerza bruta`);
    });

    next();
  }
}


