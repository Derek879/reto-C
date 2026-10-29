# Recepción de Celulares · API REST en .NET 8

API para talleres de reparación de celulares: recepción de equipos, órdenes de servicio con número correlativo,
stock de repuestos y reporte de ganancias. Es la versión en **C# / ASP.NET Core** de un sistema que hoy está en
piloto en un negocio familiar (la versión original está hecha en Python/FastAPI).

**Stack:** C# 12 · ASP.NET Core 8 Web API · Entity Framework Core 8 · SQL Server · JWT · BCrypt · xUnit · Swagger · Docker · GitHub Actions

## Qué hace

| Módulo | Endpoints | Reglas de negocio |
|---|---|---|
| Autenticación | `POST /api/auth/registro`, `POST /api/auth/login` | Contraseñas con BCrypt, token JWT con el negocio y el rol del usuario |
| Clientes | `GET/POST/PUT /api/clientes` | DNI de 8 dígitos, celular peruano de 9 dígitos, DNI único por negocio |
| Repuestos | `GET/POST /api/repuestos`, `POST /api/repuestos/{id}/ajuste` | El stock nunca queda negativo (validación + `CHECK` en SQL Server) |
| Órdenes | `GET/POST /api/ordenes`, `PATCH /{id}/estado`, `POST /{id}/repuestos`, `POST /{id}/anular` | Numeración correlativa que nunca se repite, flujo de estados, anular en vez de borrar |
| Reportes | `GET /api/reportes/ganancias?desde=&hasta=` | Ganancia = cobrado − costo de repuestos, agrupado por día |

### Decisiones de diseño

- **Multi-negocio:** cada taller solo ve sus datos. Se resuelve una sola vez con *global query filters* de EF Core
  usando el `negocio_id` del JWT, en vez de repetir `WHERE NegocioId = ...` en cada consulta.
- **Correlativo seguro:** el número de orden se incrementa con un `UPDATE` atómico dentro de una transacción, así dos
  recepciones al mismo tiempo nunca reciben el mismo número. Además hay un índice único `(NegocioId, Numero)`.
- **Anular, no borrar:** una orden anulada guarda el motivo, devuelve los repuestos al stock y su número queda reservado.
- **Flujo de estados:** `Recibido → EnReparacion → Listo → Entregado`. No se puede saltar pasos ni modificar una orden entregada.
- **Costo histórico:** cada repuesto usado guarda su costo del momento, para que los reportes pasados no cambien.
- **Errores claros:** las reglas de negocio responden `400` con `ProblemDetails` (RFC 7807); recursos de otro negocio responden `404`.
- **Aviso legal:** cada orden indica que no es comprobante de pago SUNAT.

## Cómo ejecutarlo

### Opción 1: Docker (SQL Server incluido)

```bash
docker compose up --build
```

Abre http://localhost:8080/swagger, registra un negocio en `/api/auth/registro`, copia el token y pégalo en **Authorize**.

### Opción 2: Visual Studio / LocalDB

1. Abre `RecepcionCelulares.sln` en Visual Studio 2022.
2. La cadena de conexión por defecto usa `(localdb)\MSSQLLocalDB`. En desarrollo la base de datos se crea sola con las migraciones.
3. Ejecuta con F5 y se abre Swagger.

Para crear o actualizar la base manualmente:

```bash
dotnet tool restore
dotnet ef database update -p src/RecepcionCelulares.Api
```

## Pruebas

```bash
dotnet test
```

17 pruebas: unitarias (flujo de estados, saldo) y de integración que levantan la API completa con
`WebApplicationFactory` sobre SQLite en memoria (login, aislamiento entre negocios, correlativo, stock, reportes).

## Estructura

```
src/RecepcionCelulares.Api
├── Controllers/      Endpoints REST
├── Domain/           Entidades y reglas (OrdenServicio, Repuesto, Cliente...)
├── Data/             AppDbContext, configuración Fluent API y migraciones
├── Services/         Lógica de negocio (órdenes, autenticación, reportes)
├── Dtos/             Contratos de entrada/salida con validaciones
└── Infrastructure/   Manejo de errores y negocio actual desde el JWT
tests/RecepcionCelulares.Tests
```
