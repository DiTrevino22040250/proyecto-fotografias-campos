export interface EmailPort {
  enviarCorreoListo(email: string, nombre: string, servicio: string): Promise<void>;
}
export const EMAIL_PORT = 'EMAIL_PORT';
