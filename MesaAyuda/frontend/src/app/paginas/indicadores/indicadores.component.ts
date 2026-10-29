import { Component, OnInit, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { TicketsService } from '../../core/tickets.service';
import { ConteoPor, ETIQUETA_PRIORIDAD, Indicadores, Prioridad } from '../../core/modelos';
import { mensajesDeError } from '../../core/errores';

@Component({
  selector: 'app-indicadores',
  standalone: true,
  imports: [DecimalPipe],
  templateUrl: './indicadores.component.html',
  styleUrl: './indicadores.component.css',
})
export class IndicadoresComponent implements OnInit {
  private readonly tickets = inject(TicketsService);

  readonly datos = signal<Indicadores | null>(null);
  readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.tickets.indicadores().subscribe({
      next: d => this.datos.set(d),
      error: e => this.error.set(mensajesDeError(e)[0]),
    });
  }

  ancho(item: ConteoPor, lista: ConteoPor[]): number {
    const max = Math.max(...lista.map(i => i.cantidad), 1);
    return (item.cantidad / max) * 100;
  }

  etiqueta(clave: string): string {
    return ETIQUETA_PRIORIDAD[clave as Prioridad] ?? clave;
  }
}
