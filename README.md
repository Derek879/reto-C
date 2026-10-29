# Proyectos en C# / .NET · Derek Parian Rojas

Estudiante de Diseño y Desarrollo de Software en Tecsup (Lima). Este repositorio reúne tres proyectos en .NET 8
con SQL Server, cada uno con pruebas automáticas, Docker y CI en GitHub Actions.

| Proyecto | Qué es | Tecnologías |
|---|---|---|
| [RecepcionCelulares](RecepcionCelulares) | API REST para talleres de reparación: órdenes de servicio, stock de repuestos, reportes de ganancias. Versión .NET de un sistema que está en piloto en un negocio familiar. | ASP.NET Core Web API, Entity Framework Core, SQL Server, JWT, BCrypt, Swagger, xUnit |
| [InventarioMvc](InventarioMvc) | Aplicación web de inventario y ventas con la lógica de datos en procedimientos almacenados. | ASP.NET Core MVC, Razor, SQL Server, T-SQL, Dapper, Bootstrap, Chart.js, Excel |
| [MesaAyuda](MesaAyuda) | Mesa de ayuda de TI: tickets con SLA por prioridad, bandeja priorizada, monitor de vencimientos e indicadores. | Clean Architecture, ASP.NET Core Web API, EF Core, SQL Server, FluentValidation, Angular 18, TypeScript |

Todos se levantan con `docker compose up --build` dentro de su carpeta, o se abren en Visual Studio 2022 con LocalDB.

Contacto: [github.com/Derek879](https://github.com/Derek879)
