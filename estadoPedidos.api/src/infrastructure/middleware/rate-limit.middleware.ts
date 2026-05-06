import { Injectable, NestMiddleware } from '@nestjs/common';
import { Request, Response, NextFunction } from 'express';

const cache = new Map<string, { count: number; expires: number }>();

@Injectable()
export class RateLimitMiddleware implements NestMiddleware {
  use(req: Request, res: Response, next: NextFunction) {
    const ip = req.ip || 'unknown';
    const now = Date.now();
    const limit = 50; // Max 50 peticiones por minuto
    const window = 60000;

    const userData = cache.get(ip) || { count: 0, expires: now + window };

    if (now > userData.expires) {
      userData.count = 1;
      userData.expires = now + window;
    } else {
      userData.count++;
    }

    cache.set(ip, userData);

    if (userData.count > limit) {
      return res.status(429).json({ message: 'Demasiadas peticiones. Intenta más tarde.' });
    }
    next();
  }
}



