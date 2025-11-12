import { AsyncPipe, CommonModule, DatePipe, NgClass, NgIf } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  computed,
  inject,
  signal
} from '@angular/core';
import {
  FormBuilder,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { debounceTime } from 'rxjs/operators';

import { TurnsApiService } from '../../core/services/turns-api.service';
import { MedicsApiService } from '../../core/services/medics-api.service';
import { TimeTurnsApiService } from '../../core/services/time-turns-api.service';
import { MedicDto } from '../../core/models/medic.model';
import { TimeTurnDto } from '../../core/models/time-turn.model';
import { TurnCreateRequest, TurnDto, TurnUpdateRequest } from '../../core/models/turn.model';

@Component({
  selector: 'app-turns-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, NgIf, AsyncPipe, DatePipe, NgClass],
  templateUrl: './turns-page.component.html',
  styleUrls: ['./turns-page.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TurnsPageComponent implements OnInit {
  private readonly turnsApi = inject(TurnsApiService);
  private readonly medicsApi = inject(MedicsApiService);
  private readonly timeTurnsApi = inject(TimeTurnsApiService);
  private readonly fb = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  readonly medics = signal<MedicDto[]>([]);
  readonly timeTurns = signal<TimeTurnDto[]>([]);
  readonly turns = signal<TurnDto[]>([]);
  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);
  readonly editingTurnId = signal<string | null>(null);

  readonly isEditing = computed(() => this.editingTurnId() !== null);

  readonly filterForm = this.fb.group({
    date: new FormControl<string>(this.formatDateInput(new Date()), { nonNullable: true }),
    medicId: new FormControl<string | null>(null),
    includeAccessed: new FormControl<boolean>(false, { nonNullable: true })
  });

  readonly turnForm: FormGroup = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    dni: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(10)]],
    medicId: ['', Validators.required],
    dateTurn: ['', Validators.required],
    timeId: ['', Validators.required],
    socialWork: [''],
    reason: [''],
    accessed: [false]
  });

  constructor() {}

  ngOnInit(): void {
    this.loadMetadata();
    this.startCreate();
    this.reloadTurns();
    this.filterForm.valueChanges
      .pipe(debounceTime(200), takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.reloadTurns());
  }

  private loadMetadata(): void {
    this.medicsApi
      .getMedics()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (medics) => this.medics.set(medics),
        error: () => this.errorMessage.set('No se pudieron cargar los profesionales disponibles.')
      });

    this.timeTurnsApi
      .getTimeTurns()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (timeTurns) => this.timeTurns.set(timeTurns),
        error: () => this.errorMessage.set('No se pudieron cargar los horarios disponibles.')
      });
  }

  private loadTurns(date?: string | null, medicId?: string, includeAccessed?: boolean): void {
    this.loading.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.turnsApi
      .getTurns({ date: date ?? undefined, medicId, includeAccessed })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (turns) => {
          this.turns.set(turns);
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
          this.errorMessage.set('No se pudieron recuperar los turnos. Intente nuevamente.');
        }
      });
  }

  startCreate(): void {
    this.editingTurnId.set(null);
    this.turnForm.reset({
      name: '',
      dni: '',
      medicId: '',
      dateTurn: this.filterForm.get('date')?.value ?? this.formatDateInput(new Date()),
      timeId: '',
      socialWork: '',
      reason: '',
      accessed: false
    });
  }

  editTurn(turn: TurnDto): void {
    this.editingTurnId.set(turn.id);
    this.turnForm.setValue({
      name: turn.name,
      dni: turn.dni,
      medicId: turn.medicId,
      dateTurn: this.toInputDate(turn.date),
      timeId: turn.timeId,
      socialWork: turn.socialWork ?? '',
      reason: turn.reason ?? '',
      accessed: turn.accessed
    });
  }

  cancelEdition(): void {
    this.startCreate();
  }

  submitTurn(): void {
    if (this.turnForm.invalid) {
      this.turnForm.markAllAsTouched();
      return;
    }

    const formValue = this.turnForm.value;
    this.loading.set(true);
    this.errorMessage.set(null);

    if (this.isEditing()) {
      const payload: TurnUpdateRequest = {
        id: this.editingTurnId()!,
        name: formValue['name']!,
        dni: formValue['dni']!,
        medicId: formValue['medicId']!,
        dateTurn: formValue['dateTurn']!,
        timeId: formValue['timeId']!,
        socialWork: formValue['socialWork'] ?? undefined,
        reason: formValue['reason'] ?? undefined,
        accessed: formValue['accessed'] ?? false
      };

      this.turnsApi
        .updateTurn(payload.id, payload)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            this.successMessage.set('Turno actualizado correctamente.');
            this.startCreate();
            this.reloadTurns();
          },
          error: () => this.handleRequestError('No se pudo actualizar el turno, intente nuevamente.')
        });
    } else {
      const payload: TurnCreateRequest = {
        name: formValue['name']!,
        dni: formValue['dni']!,
        medicId: formValue['medicId']!,
        dateTurn: formValue['dateTurn']!,
        timeId: formValue['timeId']!,
        socialWork: formValue['socialWork'] ?? undefined,
        reason: formValue['reason'] ?? undefined
      };

      this.turnsApi
        .createTurn(payload)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            this.successMessage.set('Turno creado con éxito.');
            this.startCreate();
            this.reloadTurns();
          },
          error: () => this.handleRequestError('No se pudo crear el turno, intente nuevamente.')
        });
    }
  }

  deleteTurn(turn: TurnDto): void {
    if (!confirm(`¿Desea eliminar el turno de ${turn.name}?`)) {
      return;
    }

    this.loading.set(true);
    this.turnsApi
      .deleteTurn(turn.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.successMessage.set('Turno eliminado.');
          this.reloadTurns();
        },
        error: () => this.handleRequestError('No se pudo eliminar el turno.')
      });
  }

  toggleAccessed(turn: TurnDto): void {
    this.turnsApi
      .markAsAccessed(turn.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.successMessage.set('Turno marcado como ingresado.');
          this.reloadTurns();
        },
        error: () => this.handleRequestError('No se pudo actualizar el estado del turno.')
      });
  }

  reloadTurns(): void {
    const date = this.filterForm.get('date')!.value;
    const medicId = this.filterForm.get('medicId')!.value ?? undefined;
    const includeAccessed = this.filterForm.get('includeAccessed')!.value;
    this.loadTurns(date, medicId, includeAccessed);
  }

  trackByTurnId(_: number, item: TurnDto): string {
    return item.id;
  }

  private formatDateInput(date: Date): string {
    const month = `${date.getMonth() + 1}`.padStart(2, '0');
    const day = `${date.getDate()}`.padStart(2, '0');
    return `${date.getFullYear()}-${month}-${day}`;
  }

  private toInputDate(date: string): string {
    const [year, month, day] = date.split('-');
    if (!day) {
      const parts = date.split('/');
      if (parts.length === 3) {
        return `${parts[2]}-${parts[1].padStart(2, '0')}-${parts[0].padStart(2, '0')}`;
      }
      return this.formatDateInput(new Date());
    }
    return `${year}-${month}-${day}`;
  }

  private handleRequestError(message: string): void {
    this.loading.set(false);
    this.errorMessage.set(message);
  }
}
