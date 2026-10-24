import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

import { CatalogsService, EntitiesService, EntityListItemDto, EntityTypeDto } from '../../api';
import { apiErrorMessage } from '../../core/api-error';

@Component({
  selector: 'app-entity-list',
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './entity-list.component.html'
})
export class EntityListComponent implements OnInit {
  readonly items = signal<EntityListItemDto[]>([]);
  readonly entityTypes = signal<EntityTypeDto[]>([]);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly pendingDelete = signal<EntityListItemDto | null>(null);
  readonly deleting = signal(false);

  search = '';
  entityTypeId: number | null = null;

  constructor(
    private entities: EntitiesService,
    private catalogs: CatalogsService
  ) {}

  async ngOnInit(): Promise<void> {
    try {
      this.entityTypes.set(await this.catalogs.getEntityTypes());
      await this.load();
    } catch (error) {
      this.error.set(apiErrorMessage(error));
      this.loading.set(false);
    }
  }

  async load(): Promise<void> {
    this.loading.set(true);
    this.error.set('');

    try {
      this.items.set(
        await this.entities.list({
          Search: this.search.trim() || undefined,
          EntityTypeId: this.entityTypeId ?? undefined
        })
      );
    } catch (error) {
      this.error.set(apiErrorMessage(error));
    } finally {
      this.loading.set(false);
    }
  }

  async confirmDelete(): Promise<void> {
    const item = this.pendingDelete();
    if (!item) {
      return;
    }

    this.deleting.set(true);
    this.error.set('');

    try {
      await this.entities.delete({ id: Number(item.id) });
      this.pendingDelete.set(null);
      await this.load();
    } catch (error) {
      this.error.set(apiErrorMessage(error));
    } finally {
      this.deleting.set(false);
    }
  }
}
