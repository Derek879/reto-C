import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', title: 'Bandeja de tickets', loadComponent: () => import('./paginas/bandeja/bandeja.component').then(m => m.BandejaComponent) },
  { path: 'nuevo', title: 'Nuevo ticket', loadComponent: () => import('./paginas/nuevo/nuevo.component').then(m => m.NuevoComponent) },
  { path: 'tickets/:id', title: 'Detalle del ticket', loadComponent: () => import('./paginas/detalle/detalle.component').then(m => m.DetalleComponent) },
  { path: 'indicadores', title: 'Indicadores', loadComponent: () => import('./paginas/indicadores/indicadores.component').then(m => m.IndicadoresComponent) },
  { path: '**', redirectTo: '' },
];
