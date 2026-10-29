import { HttpErrorResponse } from '@angular/common/http';
import { Problema } from './modelos';

/** Convierte un error HTTP de la API en mensajes legibles para mostrar al usuario. */
export function mensajesDeError(error: unknown): string[] {
  if (!(error instanceof HttpErrorResponse)) return ['Ocurrió un error inesperado.'];
  if (error.status === 0) return ['No se pudo conectar con el servidor.'];

  const problema = error.error as Problema | null;
  if (problema?.errors) return Object.values(problema.errors).flat();
  return [problema?.detail ?? problema?.title ?? `Error ${error.status}`];
}
