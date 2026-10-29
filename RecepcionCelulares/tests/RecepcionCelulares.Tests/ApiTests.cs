using System.Net;
using System.Net.Http.Json;
using RecepcionCelulares.Api.Domain;
using RecepcionCelulares.Api.Dtos;

namespace RecepcionCelulares.Tests;

public class ApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly System.Text.Json.JsonSerializerOptions Json = Ayudante.Json;

    [Fact]
    public async Task Sin_token_responde_401()
    {
        var resp = await factory.CreateClient().GetAsync("/api/ordenes");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Login_con_contrasena_incorrecta_responde_401()
    {
        var http = factory.CreateClient();
        await http.PostAsJsonAsync("/api/auth/registro",
            new RegistroRequest("Taller", null, "Admin", "login@taller.pe", "secreto123"), Json);

        var resp = await http.PostAsJsonAsync("/api/auth/login", new LoginRequest("login@taller.pe", "otra-clave"), Json);

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Numeracion_es_correlativa_por_negocio_y_no_se_reutiliza_al_anular()
    {
        var http = await Ayudante.NuevoNegocioAsync(factory);
        var cliente = await http.CrearClienteAsync();

        var primera = await http.CrearOrdenAsync(cliente.Id);
        await (await http.PostAsJsonAsync($"/api/ordenes/{primera.Id}/anular", new AnularOrdenRequest("Cliente desistió"), Json))
            .LeerAsync<OrdenResponse>();
        var segunda = await http.CrearOrdenAsync(cliente.Id);

        Assert.Equal("000001", primera.Numero);
        Assert.Equal("000002", segunda.Numero);

        // Otro negocio empieza su propia numeración.
        var otro = await Ayudante.NuevoNegocioAsync(factory);
        var otraOrden = await otro.CrearOrdenAsync((await otro.CrearClienteAsync()).Id);
        Assert.Equal("000001", otraOrden.Numero);
    }

    [Fact]
    public async Task Un_negocio_no_ve_los_datos_de_otro()
    {
        var tallerA = await Ayudante.NuevoNegocioAsync(factory);
        var tallerB = await Ayudante.NuevoNegocioAsync(factory);
        var orden = await tallerA.CrearOrdenAsync((await tallerA.CrearClienteAsync()).Id);

        var resp = await tallerB.GetAsync($"/api/ordenes/{orden.Id}");
        var listaB = await (await tallerB.GetAsync("/api/ordenes")).LeerAsync<PaginaResponse<OrdenResponse>>();

        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        Assert.Equal(0, listaB.Total);
    }

    [Fact]
    public async Task Usar_repuesto_descuenta_stock_y_anular_lo_devuelve()
    {
        var http = await Ayudante.NuevoNegocioAsync(factory);
        var repuesto = await http.CrearRepuestoAsync(stock: 3);
        var orden = await http.CrearOrdenAsync((await http.CrearClienteAsync()).Id);

        var conRepuesto = await (await http.PostAsJsonAsync($"/api/ordenes/{orden.Id}/repuestos",
            new AgregarRepuestoRequest(repuesto.Id, 2), Json)).LeerAsync<OrdenResponse>();
        Assert.Equal(EstadoOrden.EnReparacion, conRepuesto.Estado);
        Assert.Equal(1, await StockAsync(http, repuesto.Id));

        await (await http.PostAsJsonAsync($"/api/ordenes/{orden.Id}/anular", new AnularOrdenRequest("Equipo sin reparación"), Json))
            .LeerAsync<OrdenResponse>();
        Assert.Equal(3, await StockAsync(http, repuesto.Id));
    }

    [Fact]
    public async Task No_permite_usar_mas_repuestos_que_el_stock()
    {
        var http = await Ayudante.NuevoNegocioAsync(factory);
        var repuesto = await http.CrearRepuestoAsync(stock: 1);
        var orden = await http.CrearOrdenAsync((await http.CrearClienteAsync()).Id);

        var resp = await http.PostAsJsonAsync($"/api/ordenes/{orden.Id}/repuestos", new AgregarRepuestoRequest(repuesto.Id, 2), Json);

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        Assert.Contains("Stock insuficiente", await resp.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task No_permite_saltar_estados()
    {
        var http = await Ayudante.NuevoNegocioAsync(factory);
        var orden = await http.CrearOrdenAsync((await http.CrearClienteAsync()).Id);

        var resp = await http.PatchAsJsonAsync($"/api/ordenes/{orden.Id}/estado", new CambiarEstadoRequest(EstadoOrden.Entregado), Json);

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task Reporte_de_ganancias_resta_costo_de_repuestos()
    {
        var http = await Ayudante.NuevoNegocioAsync(factory);
        var repuesto = await http.CrearRepuestoAsync(stock: 5, costo: 80m);
        var orden = await http.CrearOrdenAsync((await http.CrearClienteAsync()).Id, precio: 250m);

        await (await http.PostAsJsonAsync($"/api/ordenes/{orden.Id}/repuestos", new AgregarRepuestoRequest(repuesto.Id, 1), Json)).LeerAsync<OrdenResponse>();
        foreach (var estado in new[] { EstadoOrden.Listo, EstadoOrden.Entregado })
            await (await http.PatchAsJsonAsync($"/api/ordenes/{orden.Id}/estado", new CambiarEstadoRequest(estado), Json)).LeerAsync<OrdenResponse>();

        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var reporte = await (await http.GetAsync($"/api/reportes/ganancias?desde={hoy:yyyy-MM-dd}&hasta={hoy:yyyy-MM-dd}"))
            .LeerAsync<ReporteGanancias>();

        Assert.Equal(1, reporte.OrdenesEntregadas);
        Assert.Equal(250m, reporte.Ingresos);
        Assert.Equal(80m, reporte.CostoRepuestos);
        Assert.Equal(170m, reporte.Ganancia);
    }

    [Fact]
    public async Task Valida_el_dni_del_cliente()
    {
        var http = await Ayudante.NuevoNegocioAsync(factory);

        var resp = await http.PostAsJsonAsync("/api/clientes", new ClienteRequest("Luis", "123", "987654321"), Json);

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    private static async Task<int> StockAsync(HttpClient http, int repuestoId) =>
        (await (await http.GetAsync("/api/repuestos")).LeerAsync<List<RepuestoResponse>>()).Single(r => r.Id == repuestoId).Stock;
}
