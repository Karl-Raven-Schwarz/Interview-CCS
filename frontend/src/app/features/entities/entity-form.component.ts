import { Component, OnInit, signal } from '@angular/core';
import { FormArray, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import {
  CatalogsService,
  DocumentTypeDto,
  EntitiesService,
  EntityCreateDto,
  EntityDetailDto,
  EntityTypeDto,
  PhoneDto
} from '../../api';
import { apiErrorMessage, apiFieldErrors } from '../../core/api-error';

type PhoneGroup = FormGroup<{
  number: FormControl<string | null>;
  label: FormControl<string | null>;
}>;

const EMAIL_PATTERN = /^[^@\s]+@[^@\s]+\.[A-Za-z]{2,}$/;
const PHONE_PATTERN = /^[0-9+\-\s()]+$/;
const MAX_PHONES = 10;

/**
 * Alta y edición de entidades (misma página). Los datos propios de persona
 * natural o jurídica se muestran según el tipo de entidad elegido; la API
 * vuelve a validar cada regla.
 */
@Component({
  selector: 'app-entity-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './entity-form.component.html'
})
export class EntityFormComponent implements OnInit {
  readonly entityTypes = signal<EntityTypeDto[]>([]);
  readonly documentTypes = signal<DocumentTypeDto[]>([]);
  readonly category = signal('N');
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly error = signal('');
  readonly fieldErrors = signal<Record<string, string[]>>({});

  /** 0 = alta; mayor que 0 = edición. */
  entityId = 0;

  readonly form = new FormGroup({
    entityTypeId: new FormControl<number | null>(null, [Validators.required]),
    documentTypeId: new FormControl<number | null>(null, [Validators.required]),
    documentNumber: new FormControl('', [
      Validators.required,
      Validators.minLength(6),
      Validators.maxLength(20)
    ]),
    email: new FormControl('', [Validators.email, Validators.pattern(EMAIL_PATTERN)]),
    address: new FormControl('', [Validators.maxLength(150)]),
    names: new FormControl('', [Validators.maxLength(100)]),
    paternalSurname: new FormControl('', [Validators.maxLength(60)]),
    maternalSurname: new FormControl('', [Validators.maxLength(60)]),
    age: new FormControl<number | null>(null, [Validators.min(1), Validators.max(130)]),
    sex: new FormControl<string | null>(null),
    legalName: new FormControl('', [Validators.maxLength(150)]),
    tradeName: new FormControl('', [Validators.maxLength(150)]),
    contactName: new FormControl('', [Validators.maxLength(120)]),
    isActive: new FormControl(true),
    phones: new FormArray<PhoneGroup>([])
  });

  get phones(): FormArray<PhoneGroup> {
    return this.form.controls.phones;
  }

  constructor(
    private catalogs: CatalogsService,
    private entities: EntitiesService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  get isEdit(): boolean {
    return this.entityId > 0;
  }

  async ngOnInit(): Promise<void> {
    this.entityId = Number(this.route.snapshot.paramMap.get('id')) || 0;

    try {
      this.entityTypes.set(await this.catalogs.getEntityTypes());

      if (this.isEdit) {
        await this.patchForm(await this.entities.getById({ id: this.entityId }));
      } else {
        const first = this.entityTypes()[0];
        if (first) {
          this.form.controls.entityTypeId.setValue(Number(first.id));
          await this.onTypeChange();
        }
      }
    } catch (error) {
      this.error.set(apiErrorMessage(error));
    } finally {
      this.loading.set(false);
    }
  }

  /** Cambia la categoría (N/L): ajusta validaciones y recarga tipos de documento. */
  async onTypeChange(): Promise<void> {
    await this.applyType(this.form.controls.entityTypeId.value);
  }

  addPhone(phone?: PhoneDto): void {
    if (this.phones.length >= MAX_PHONES) {
      return;
    }

    this.phones.push(
      new FormGroup({
        number: new FormControl(phone?.number ?? '', [
          Validators.required,
          Validators.minLength(6),
          Validators.maxLength(20),
          Validators.pattern(PHONE_PATTERN)
        ]),
        label: new FormControl<string | null>(phone?.label ?? null, [Validators.maxLength(30)])
      })
    );
  }

  removePhone(index: number): void {
    this.phones.removeAt(index);
  }

  async save(): Promise<void> {
    this.error.set('');
    this.fieldErrors.set({});

    // Se descartan las filas de teléfono vacías (el alta trae una en blanco).
    for (let index = this.phones.length - 1; index >= 0; index--) {
      const phone = this.phones.at(index).value;
      if (!phone.number?.trim() && !phone.label?.trim()) {
        this.phones.removeAt(index);
      }
    }

    if (this.form.invalid || this.phones.invalid) {
      this.form.markAllAsTouched();
      this.phones.markAllAsTouched();
      return;
    }

    this.saving.set(true);

    try {
      const payload = this.buildPayload();

      if (this.isEdit) {
        await this.entities.update({
          id: this.entityId,
          body: { ...payload, isActive: this.form.controls.isActive.value ?? true }
        });
      } else {
        await this.entities.create({ body: payload });
      }

      this.router.navigate(['/entities']);
    } catch (error) {
      this.error.set(apiErrorMessage(error));
      this.fieldErrors.set(apiFieldErrors(error));
    } finally {
      this.saving.set(false);
    }
  }

  /** Primer mensaje de error del campo (las claves del contrato vienen en cualquier caja). */
  fieldMessage(name: string): string | null {
    const errors = this.fieldErrors();
    const key = Object.keys(errors).find((candidate) => candidate.toLowerCase() === name.toLowerCase());
    return key ? errors[key].join(' ') : null;
  }

  private async applyType(entityTypeId: number | string | null): Promise<void> {
    // El select entrega el id como texto y patchForm como número: se comparan como número.
    const type =
      this.entityTypes().find((candidate) => Number(candidate.id) === Number(entityTypeId)) ?? null;
    const category = type?.category ?? 'N';

    this.category.set(category);
    this.setCategoryValidators(category);
    this.documentTypes.set(await this.catalogs.getDocumentTypes({ category }));

    const current = this.form.controls.documentTypeId.value;
    if (current && !this.documentTypes().some((candidate) => Number(candidate.id) === Number(current))) {
      this.form.controls.documentTypeId.setValue(null);
    }
  }

  private setCategoryValidators(category: string): void {
    const natural = category === 'N';
    const controls = this.form.controls;

    controls.names.setValidators(natural ? [Validators.required, Validators.maxLength(100)] : [Validators.maxLength(100)]);
    controls.paternalSurname.setValidators(natural ? [Validators.required, Validators.maxLength(60)] : [Validators.maxLength(60)]);
    controls.maternalSurname.setValidators(natural ? [Validators.required, Validators.maxLength(60)] : [Validators.maxLength(60)]);
    controls.age.setValidators(natural ? [Validators.required, Validators.min(1), Validators.max(130)] : [Validators.min(1), Validators.max(130)]);
    controls.sex.setValidators(natural ? [Validators.required] : []);
    controls.legalName.setValidators(natural ? [Validators.maxLength(150)] : [Validators.required, Validators.maxLength(150)]);

    controls.names.updateValueAndValidity();
    controls.paternalSurname.updateValueAndValidity();
    controls.maternalSurname.updateValueAndValidity();
    controls.age.updateValueAndValidity();
    controls.sex.updateValueAndValidity();
    controls.legalName.updateValueAndValidity();
  }

  private async patchForm(entity: EntityDetailDto): Promise<void> {
    for (const phone of entity.phones ?? []) {
      this.addPhone(phone);
    }

    this.form.patchValue({
      entityTypeId: Number(entity.entityTypeId),
      documentTypeId: Number(entity.documentTypeId),
      documentNumber: entity.documentNumber ?? '',
      email: entity.email ?? '',
      address: entity.address ?? '',
      names: entity.names ?? '',
      paternalSurname: entity.paternalSurname ?? '',
      maternalSurname: entity.maternalSurname ?? '',
      age: entity.age === null || entity.age === undefined ? null : Number(entity.age),
      sex: entity.sex ?? null,
      legalName: entity.legalName ?? '',
      tradeName: entity.tradeName ?? '',
      contactName: entity.contactName ?? '',
      isActive: entity.isActive ?? true
    });

    await this.applyType(Number(entity.entityTypeId));
  }

  private buildPayload(): EntityCreateDto {
    const value = this.form.getRawValue();
    const natural = this.category() === 'N';

    return {
      entityTypeId: Number(value.entityTypeId),
      documentTypeId: Number(value.documentTypeId),
      documentNumber: (value.documentNumber ?? '').trim(),
      email: value.email?.trim() || null,
      address: value.address?.trim() || null,
      names: natural ? value.names?.trim() : null,
      paternalSurname: natural ? value.paternalSurname?.trim() : null,
      maternalSurname: natural ? value.maternalSurname?.trim() : null,
      age: natural ? value.age : null,
      sex: natural ? value.sex : null,
      legalName: natural ? null : value.legalName?.trim(),
      tradeName: natural ? null : value.tradeName?.trim(),
      contactName: natural ? null : value.contactName?.trim(),
      phones: this.phones.controls.map((phone) => ({
        number: (phone.value.number ?? '').trim(),
        label: phone.value.label?.trim() || null
      }))
    };
  }
}
