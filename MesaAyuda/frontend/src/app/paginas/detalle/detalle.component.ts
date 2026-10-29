import { Component, OnInit, inject, input, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Observable } from 'rxjs';
import { TicketsService } from '../../core/tickets.service';
import { ETIQUETA_ESTADO, ETIQUETA_PRIORIDAD, Prioridad, PRIORIDADES, TicketDetalle } from '../../core/modelos';
import { SlaPipe } from '../../core/sla.pipe';
import { mensajesDeError } from '../../core/errores';

@Component({
  selector: 'app-detalle',
  standalone: true,
  imports: [FormsModule, RouterLink, DatePipe, SlaPipe],
  templateUrl: './detalle.component.html',
  styleUrl: './detalle.component.css',
})
export class DetalleComponent implements OnInit {
  private readonly tickets = inject(TicketsService);

  /** Viene del parámetro :id de la ruta (withComponentInputBinding). */
  readonly id = input.required<string>();

  readonly etiquetaEstado = ETIQUETA_ESTADO;
  readonly etiquetaPrioridad = ETIQUETA_PRIORIDAD;
  readonly prioridades = PRIORIDADES;

  readonly ticket = signal<TicketDetalle | null>(null);
  readonly error = signal<string | null>(null);
  readonly ocupado = signal(false);
  readonly ahora = Date.now();

  tecnico = '';
  texto = '';
  autor = '';
  comentario = '';

  ngOnInit(): void {
    this.tickets.obtener(+this.id()).subscribe({
      next: t => this.ticket.set(t),
      error: e => this.error.set(mensajesDeError(e)[0]),
    });
  }

  get activo(): boolean {
    const t = this.ticket();
    return !!t && t.estado !== 'Resuelto' && t.estado !== 'Cerrado';
  }

  asignar(): void {
    this.ejecutar(this.tickets.asignar(this.idNum, this.tecnico), () => (this.tecnico = ''));
  }

  cambiarPrioridad(prioridad: Prioridad): void {
    this.ejecutar(this.tickets.cambiarPrioridad(this.idNum, prioridad));
  }

  ponerEnEspera(): void {
    this.ejecutar(this.tickets.ponerEnEspera(this.idNum, this.texto), () => (this.texto = ''));
  }

  retomar(): void {
    this.ejecutar(this.tickets.retomar(this.idNum));
  }

  resolver(): void {
    this.ejecutar(this.tickets.resolver(this.idNum, this.texto), () => (this.texto = ''));
  }

  reabrir(): void {
    this.ejecutar(this.tickets.reabrir(this.idNum, this.texto), () => (this.texto = ''));
  }

  cerrar(): void {
    this.ejecutar(this.tickets.cerrar(this.idNum));
  }

  comentar(): void {
    this.ejecutar(this.tickets.comentar(this.idNum, this.autor, this.comentario), () => (this.comentario = ''));
  }

  private get idNum(): number {
    return +this.id();
  }

  private ejecutar(accion: Observable<TicketDetalle>, alTerminar?: () => void): void {
    this.ocupado.set(true);
    this.error.set(null);
    accion.subscribe({
      next: t => {
        this.ticket.set(t);
        this.ocupado.set(false);
        alTerminar?.();
      },
      error: e => {
        this.error.set(mensajesDeError(e).join(' '));
        this.ocupado.set(false);
      },
    });
  }
}
