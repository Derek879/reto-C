import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { CrearTicket, FiltroTickets, Indicadores, Pagina, Prioridad, TicketDetalle, TicketResumen } from './modelos';

@Injectable({ providedIn: 'root' })
export class TicketsService {
  private readonly http = inject(HttpClient);
  private readonly api = '/api';

  buscar(filtro: FiltroTickets): Observable<Pagina<TicketResumen>> {
    let params = new HttpParams().set('tamanoPagina', 15);
    for (const [clave, valor] of Object.entries(filtro)) {
      if (valor !== '' && valor !== undefined && valor !== null && valor !== false) {
        params = params.set(clave, String(valor));
      }
    }
    return this.http.get<Pagina<TicketResumen>>(`${this.api}/tickets`, { params });
  }

  obtener(id: number): Observable<TicketDetalle> {
    return this.http.get<TicketDetalle>(`${this.api}/tickets/${id}`);
  }

  crear(ticket: CrearTicket): Observable<TicketDetalle> {
    return this.http.post<TicketDetalle>(`${this.api}/tickets`, ticket);
  }

  asignar(id: number, tecnico: string) {
    return this.accion(id, 'asignar', { tecnico });
  }

  cambiarPrioridad(id: number, prioridad: Prioridad) {
    return this.accion(id, 'prioridad', { prioridad });
  }

  ponerEnEspera(id: number, texto: string) {
    return this.accion(id, 'en-espera', { texto });
  }

  retomar(id: number) {
    return this.accion(id, 'retomar');
  }

  resolver(id: number, texto: string) {
    return this.accion(id, 'resolver', { texto });
  }

  reabrir(id: number, texto: string) {
    return this.accion(id, 'reabrir', { texto });
  }

  cerrar(id: number) {
    return this.accion(id, 'cerrar');
  }

  comentar(id: number, autor: string, texto: string) {
    return this.accion(id, 'comentarios', { autor, texto });
  }

  indicadores(): Observable<Indicadores> {
    return this.http.get<Indicadores>(`${this.api}/indicadores`);
  }

  private accion(id: number, ruta: string, cuerpo: object = {}): Observable<TicketDetalle> {
    return this.http.post<TicketDetalle>(`${this.api}/tickets/${id}/${ruta}`, cuerpo);
  }
}
