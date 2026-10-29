import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { TicketsService } from '../../core/tickets.service';
import { CATEGORIAS, ETIQUETA_PRIORIDAD, PRIORIDADES } from '../../core/modelos';
import { mensajesDeError } from '../../core/errores';

@Component({
  selector: 'app-nuevo',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './nuevo.component.html',
})
export class NuevoComponent {
  private readonly tickets = inject(TicketsService);
  private readonly router = inject(Router);

  readonly categorias = CATEGORIAS;
  readonly prioridades = PRIORIDADES;
  readonly etiquetaPrioridad = ETIQUETA_PRIORIDAD;
  readonly horasSla: Record<string, number> = { Critica: 4, Alta: 8, Media: 24, Baja: 72 };

  readonly form = inject(FormBuilder).nonNullable.group({
    titulo: ['', [Validators.required, Validators.maxLength(150)]],
    descripcion: ['', [Validators.required, Validators.minLength(15), Validators.maxLength(2000)]],
    categoria: ['Software' as const, Validators.required],
    prioridad: ['Media' as const, Validators.required],
    solicitante: ['', [Validators.required, Validators.maxLength(100)]],
  });

  readonly enviando = signal(false);
  readonly errores = signal<string[]>([]);

  invalido(campo: keyof typeof this.form.controls): boolean {
    const c = this.form.controls[campo];
    return c.invalid && (c.touched || c.dirty);
  }

  guardar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.enviando.set(true);
    this.errores.set([]);
    this.tickets.crear(this.form.getRawValue()).subscribe({
      next: t => this.router.navigate(['/tickets', t.id]),
      error: e => {
        this.errores.set(mensajesDeError(e));
        this.enviando.set(false);
      },
    });
  }
}
