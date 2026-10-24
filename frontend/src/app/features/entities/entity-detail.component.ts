import { DatePipe } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

import { EntitiesService, EntityDetailDto } from '../../api';
import { apiErrorMessage } from '../../core/api-error';

/**
 * Vista de solo lectura de una entidad: identidad, datos de persona natural
 * o jurídica, contacto con la lista completa de teléfonos y fechas.
 */
@Component({
  selector: 'app-entity-detail',
  standalone: true,
  imports: [DatePipe, RouterLink],
  templateUrl: './entity-detail.component.html'
})
export class EntityDetailComponent implements OnInit {
  readonly entity = signal<EntityDetailDto | null>(null);
  readonly loading = signal(true);
  readonly error = signal('');

  entityId = 0;

  constructor(
    private entities: EntitiesService,
    private route: ActivatedRoute
  ) {}

  async ngOnInit(): Promise<void> {
    this.entityId = Number(this.route.snapshot.paramMap.get('id')) || 0;

    try {
      this.entity.set(await this.entities.getById({ id: this.entityId }));
    } catch (error) {
      this.error.set(apiErrorMessage(error));
    } finally {
      this.loading.set(false);
    }
  }

  /** Nombre para mostrar: "Apellidos, Nombres" o la razón social. */
  get displayName(): string {
    const entity = this.entity();
    if (!entity) {
      return '';
    }

    if (entity.entityCategory === 'N') {
      const surnames = [entity.paternalSurname, entity.maternalSurname].filter(Boolean).join(' ');
      return [surnames, entity.names].filter(Boolean).join(', ') || '—';
    }

    return entity.legalName || '—';
  }

  get isNatural(): boolean {
    return this.entity()?.entityCategory === 'N';
  }

  /** Sexo en texto: M = Masculino, F = Femenino. */
  get sexLabel(): string {
    const sex = this.entity()?.sex;
    if (sex === 'M') {
      return 'Masculino';
    }
    if (sex === 'F') {
      return 'Femenino';
    }
    return sex || '—';
  }
}
