export type Prioridad = 'Critica' | 'Alta' | 'Media' | 'Baja';
export type Categoria = 'Hardware' | 'Software' | 'Red' | 'Accesos' | 'Otro';
export type EstadoTicket = 'Abierto' | 'EnProgreso' | 'EnEspera' | 'Resuelto' | 'Cerrado';

export const PRIORIDADES: Prioridad[] = ['Critica', 'Alta', 'Media', 'Baja'];
export const CATEGORIAS: Categoria[] = ['Hardware', 'Software', 'Red', 'Accesos', 'Otro'];
export const ESTADOS: EstadoTicket[] = ['Abierto', 'EnProgreso', 'EnEspera', 'Resuelto', 'Cerrado'];

export const ETIQUETA_ESTADO: Record<EstadoTicket, string> = {
  Abierto: 'Abierto',
  EnProgreso: 'En progreso',
  EnEspera: 'En espera',
  Resuelto: 'Resuelto',
  Cerrado: 'Cerrado',
};

export const ETIQUETA_PRIORIDAD: Record<Prioridad, string> = {
  Critica: 'Crítica',
  Alta: 'Alta',
  Media: 'Media',
  Baja: 'Baja',
};

export interface TicketResumen {
  id: number;
  codigo: string;
  titulo: string;
  categoria: Categoria;
  prioridad: Prioridad;
  estado: EstadoTicket;
  solicitante: string;
  asignadoA: string | null;
  fechaCreacion: string;
  fechaLimite: string;
  slaIncumplido: boolean;
}

export interface Comentario {
  autor: string;
  texto: string;
  fecha: string;
}

export interface TicketDetalle extends TicketResumen {
  descripcion: string;
  fechaResolucion: string | null;
  solucion: string | null;
  comentarios: Comentario[];
}

export interface Pagina<T> {
  items: T[];
  total: number;
  numeroPagina: number;
  tamanoPagina: number;
}

export interface FiltroTickets {
  estado?: EstadoTicket | '';
  prioridad?: Prioridad | '';
  categoria?: Categoria | '';
  buscar?: string;
  soloVencidos?: boolean;
  pagina?: number;
}

export interface CrearTicket {
  titulo: string;
  descripcion: string;
  categoria: Categoria;
  prioridad: Prioridad;
  solicitante: string;
}

export interface ConteoPor {
  clave: string;
  cantidad: number;
}

export interface Indicadores {
  activos: number;
  vencidosActivos: number;
  resueltosUltimos30Dias: number;
  cumplimientoSla: number;
  horasPromedioResolucion: number;
  activosPorPrioridad: ConteoPor[];
  activosPorCategoria: ConteoPor[];
  activosPorTecnico: ConteoPor[];
}

/** Respuesta de error de la API (ProblemDetails). */
export interface Problema {
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
}
