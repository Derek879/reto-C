import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TicketsService } from '../../core/tickets.service';
import { CATEGORIAS, ESTADOS, ETIQUETA_ESTADO, ETIQUETA_PRIORIDAD, FiltroTickets, Pagina, PRIORIDADES, TicketResumen } from '../../core/modelos';
import { SlaPipe } from '../../core/sla.pipe';
import { mensajesDeError } from '../../core/errores';

@Component({
  selector: 'app-bandeja',
  standalone: true,
  imports: [FormsModule, RouterLink, DatePipe, SlaPipe],
  templateUrl: './bandeja.component.html',
  styleUrl: './bandeja.component.css',
})
export class BandejaComponent implements OnInit {
  private readonly tickets = inject(TicketsService);

  readonly estados = ESTADOS;
  readonly prioridades = PRIORIDADES;
  readonly categorias = CATEGORIAS;
  readonly etiquetaEstado = ETIQUETA_ESTADO;
  readonly etiquetaPrioridad = ETIQUETA_PRIORIDAD;

  filtro: FiltroTickets = { estado: '', prioridad: '', categoria: '', buscar: '', soloVencidos: false, pagina: 1 };
  readonly pagina = signal<Pagina<TicketResumen> | null>(null);
  readonly cargando = signal(false);
  readonly error = signal<string | null>(null);
  readonly ahora = signal(Date.now());

  ngOnInit(): void {
    this.cargar();
  }

  aplicarFiltros(): void {
    this.filtro.pagina = 1;
    this.cargar();
  }

  irAPagina(numero: number): void {
    this.filtro.pagina = numero;
    this.cargar();
  }

  get totalPaginas(): number {
    const p = this.pagina();
    return p ? Math.max(1, Math.ceil(p.total / p.tamanoPagina)) : 1;
  }

  esActivo(t: TicketResumen): boolean {
    return t.estado !== 'Resuelto' && t.estado !== 'Cerrado';
  }

  vencido(t: TicketResumen): boolean {
    return this.esActivo(t) && new Date(t.fechaLimite).getTime() < this.ahora();
  }

  private cargar(): void {
    this.cargando.set(true);
    this.error.set(null);
    this.ahora.set(Date.now());
    this.tickets.buscar(this.filtro).subscribe({
      next: p => {
        this.pagina.set(p);
        this.cargando.set(false);
      },
      error: e => {
        this.error.set(mensajesDeError(e)[0]);
        this.cargando.set(false);
      },
    });
  }
}
