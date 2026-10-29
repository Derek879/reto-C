using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using MesaAyuda.Application.Tickets;
using MesaAyuda.Domain;
using MesaAyuda.Infrastructure.Persistencia;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MesaAyuda.Tests;

/// <summary>API completa en memoria con SQLite y un reloj controlable.</summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _conexion = new("DataSource=:memory:");
    public RelojFalso Reloj { get; } = new(new DateTimeOffset(2026, 10, 29, 8, 0, 0, TimeSpan.Zero));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Sla:MonitorActivo", "false");
        builder.UseSetting("Logging:LogLevel:Default", "Warning");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<MesaAyudaDbContext>>();
            _conexion.Open();
            services.AddDbContext<MesaAyudaDbContext>(o => o.UseSqlite(_conexion));
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Reloj);

            using var scope = services.BuildServiceProvider().CreateScope();
            scope.ServiceProvider.GetRequiredService<MesaAyudaDbContext>().Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _conexion.Dispose();
    }
}

public class ApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };
    private readonly HttpClient _http = factory.CreateClient();

    private async Task<TicketDetalle> CrearAsync(Prioridad prioridad = Prioridad.Media)
    {
        var resp = await _http.PostAsJsonAsync("/api/tickets",
            new CrearTicketDto("Sin internet", "No hay conexión a internet en contabilidad.", Categoria.Red, prioridad, "Ana Ruiz"), Json);
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
        return (await resp.Content.ReadFromJsonAsync<TicketDetalle>(Json))!;
    }

    private async Task<TicketDetalle> PostAsync(string url, object? cuerpo = null)
    {
        var resp = await _http.PostAsJsonAsync(url, cuerpo ?? new { }, Json);
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<TicketDetalle>(Json))!;
    }

    [Fact]
    public async Task Crear_asigna_codigo_y_fecha_limite()
    {
        var t = await CrearAsync(Prioridad.Critica);

        Assert.Equal($"INC-{t.Id:D6}", t.Codigo);
        Assert.Equal(t.FechaCreacion.AddHours(4), t.FechaLimite);
        Assert.Equal(EstadoTicket.Abierto, t.Estado);
    }

    [Fact]
    public async Task Datos_invalidos_devuelven_400_con_el_detalle_por_campo()
    {
        var resp = await _http.PostAsJsonAsync("/api/tickets",
            new CrearTicketDto("", "corto", Categoria.Red, Prioridad.Media, ""), Json);
        var cuerpo = await resp.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        Assert.Contains("Titulo", cuerpo);
        Assert.Contains("Descripcion", cuerpo);
        Assert.Contains("Solicitante", cuerpo);
    }

    [Fact]
    public async Task Regla_de_dominio_violada_devuelve_409()
    {
        var t = await CrearAsync();

        var resp = await _http.PostAsJsonAsync($"/api/tickets/{t.Id}/cerrar", new { }, Json);

        Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
    }

    [Fact]
    public async Task Ticket_inexistente_devuelve_404()
    {
        Assert.Equal(HttpStatusCode.NotFound, (await _http.GetAsync("/api/tickets/999999")).StatusCode);
    }

    [Fact]
    public async Task Flujo_de_atencion_guarda_historial()
    {
        var t = await CrearAsync();
        await PostAsync($"/api/tickets/{t.Id}/asignar", new AsignarDto("Luis Paredes"));
        await PostAsync($"/api/tickets/{t.Id}/comentarios", new ComentarioDto("Luis Paredes", "Revisando el switch del piso 3."));
        var resuelto = await PostAsync($"/api/tickets/{t.Id}/resolver", new TextoDto("Se reinició el switch y se cambió un cable."));

        Assert.Equal(EstadoTicket.Resuelto, resuelto.Estado);
        Assert.Equal(3, resuelto.Comentarios.Count);

        var recargado = await _http.GetFromJsonAsync<TicketDetalle>($"/api/tickets/{t.Id}", Json);
        Assert.Equal("Luis Paredes", recargado!.AsignadoA);
        Assert.Equal(3, recargado.Comentarios.Count);
    }

    [Fact]
    public async Task Monitor_de_sla_marca_los_vencidos_y_los_filtros_los_encuentran()
    {
        var urgente = await CrearAsync(Prioridad.Critica);
        var normal = await CrearAsync(Prioridad.Baja);

        factory.Reloj.Avanzar(TimeSpan.FromHours(5));
        using (var scope = factory.Services.CreateScope())
            await scope.ServiceProvider.GetRequiredService<TicketService>().VerificarSlaAsync();

        var vencidos = await _http.GetFromJsonAsync<Pagina<TicketResumen>>("/api/tickets?soloVencidos=true&tamanoPagina=100", Json);
        Assert.Contains(vencidos!.Items, x => x.Id == urgente.Id);
        Assert.DoesNotContain(vencidos.Items, x => x.Id == normal.Id);

        var indicadores = await _http.GetFromJsonAsync<Indicadores>("/api/indicadores", Json);
        Assert.True(indicadores!.VencidosActivos >= 1);
    }

    [Fact]
    public async Task La_bandeja_muestra_primero_lo_mas_urgente()
    {
        await CrearAsync(Prioridad.Baja);
        await CrearAsync(Prioridad.Critica);

        var pagina = await _http.GetFromJsonAsync<Pagina<TicketResumen>>("/api/tickets?estado=Abierto&tamanoPagina=100", Json);
        var prioridades = pagina!.Items.Select(i => i.Prioridad).ToList();

        Assert.Equal(prioridades.OrderBy(p => p), prioridades);
    }
}
