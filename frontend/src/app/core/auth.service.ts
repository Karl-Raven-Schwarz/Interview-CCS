import { Injectable, computed, signal } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService as AuthApi, LoginRequestDto } from '../api';

const STORAGE_KEY = 'ccs.session';

/**
 * La API devuelve la expiración en UTC (puede venir con o sin zona). Se guarda
 * como ISO con zona para que la interfaz muestre la hora local. Si el valor no
 * se puede interpretar se descarta: nunca se lanza una excepción aquí.
 */
function toLocalIso(value?: string): string {
  if (!value) {
    return '';
  }

  const hasZone = /Z$|[+-]\d{2}:?\d{2}$/.test(value);
  const date = new Date(hasZone ? value : `${value}Z`);
  return Number.isNaN(date.getTime()) ? '' : date.toISOString();
}

export interface Session {
  token: string;
  username: string;
  role: string;
  expiresAt: string;
}

/**
 * Sesión del usuario. El API usa JWT sin estado, así que el logout es solo
 * cliente: se borra el token guardado.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly session = signal<Session | null>(this.read());

  readonly user = this.session.asReadonly();
  readonly isAuthenticated = computed(() => this.session() !== null);

  constructor(
    private api: AuthApi,
    private router: Router
  ) {}

  async login(username: string, password: string): Promise<void> {
    const body: LoginRequestDto = { username, password };
    const response = await this.api.login({ body });

    const token = response.token;
    if (!token) {
      throw new Error('La API no devolvió un token.');
    }

    const session: Session = {
      token,
      username: response.username ?? username,
      role: response.role ?? '',
      expiresAt: toLocalIso(response.expiresAt)
    };

    localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
    this.session.set(session);
  }

  logout(): void {
    localStorage.removeItem(STORAGE_KEY);
    this.session.set(null);
    this.router.navigate(['/login']);
  }

  private read(): Session | null {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) {
      return null;
    }

    try {
      return JSON.parse(raw) as Session;
    } catch {
      localStorage.removeItem(STORAGE_KEY);
      return null;
    }
  }
}
