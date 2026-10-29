import { Pipe, PipeTransform } from '@angular/core';

/** Muestra cuánto falta (o cuánto se pasó) para la fecha límite: "quedan 3 h 20 min" / "vencido hace 2 h". */
@Pipe({ name: 'sla', standalone: true })
export class SlaPipe implements PipeTransform {
  transform(fechaLimite: string, ahora: number = Date.now()): string {
    const diferencia = new Date(fechaLimite).getTime() - ahora;
    const texto = this.duracion(Math.abs(diferencia));
    return diferencia >= 0 ? `quedan ${texto}` : `vencido hace ${texto}`;
  }

  private duracion(ms: number): string {
    const minutos = Math.floor(ms / 60000);
    const dias = Math.floor(minutos / 1440);
    const horas = Math.floor((minutos % 1440) / 60);
    const min = minutos % 60;
    if (dias > 0) return `${dias} d ${horas} h`;
    if (horas > 0) return `${horas} h ${min} min`;
    return `${min} min`;
  }
}
