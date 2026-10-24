import { HttpErrorResponse } from '@angular/common/http';

// Contrato de errores de la API: { status, title, detail, code, errors? }
interface ApiProblem {
  detail?: string;
  errors?: Record<string, string[]>;
}

/** Mensaje para mostrar al usuario a partir de un error de la API. */
export function apiErrorMessage(error: unknown): string {
  if (error instanceof HttpErrorResponse) {
    const problem = error.error as ApiProblem | null;
    if (problem && typeof problem === 'object' && typeof problem.detail === 'string') {
      return problem.detail;
    }

    if (error.status === 0) {
      return 'No se pudo conectar con la API.';
    }

    return `Error ${error.status}: ${error.statusText || 'respuesta inesperada'}.`;
  }

  return error instanceof Error ? error.message : 'Error inesperado.';
}

/** Errores por campo (del bloque `errors` del contrato) para marcar el formulario. */
export function apiFieldErrors(error: unknown): Record<string, string[]> {
  if (error instanceof HttpErrorResponse) {
    const problem = error.error as ApiProblem | null;
    if (problem && typeof problem === 'object' && problem.errors) {
      return problem.errors;
    }
  }

  return {};
}
