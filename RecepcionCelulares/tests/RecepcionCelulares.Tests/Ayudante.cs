using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using RecepcionCelulares.Api.Domain;
using RecepcionCelulares.Api.Dtos;

namespace RecepcionCelulares.Tests;

internal static class Ayudante
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>Registra un negocio nuevo y devuelve un cliente HTTP autenticado como su administrador.</summary>
    public static async Task<HttpClient> NuevoNegocioAsync(ApiFactory factory)
    {
        var http = factory.CreateClient();
        var email = $"admin{Guid.NewGuid():N}@taller.pe";
        var resp = await http.PostAsJsonAsync("/api/auth/registro",
            new RegistroRequest("Taller de prueba", null, "Admin", email, "secreto123"), Json);
        resp.EnsureSuccessStatusCode();
        var token = (await resp.Content.ReadFromJsonAsync<TokenResponse>(Json))!.Token;
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return http;
    }

    public static async Task<T> LeerAsync<T>(this HttpResponseMessage resp)
    {
        if (!resp.IsSuccessStatusCode)
            throw new Exception($"{(int)resp.StatusCode}: {await resp.Content.ReadAsStringAsync()}");
        return (await resp.Content.ReadFromJsonAsync<T>(Json))!;
    }

    public static async Task<ClienteResponse> CrearClienteAsync(this HttpClient http, string dni = "71234567") =>
        await (await http.PostAsJsonAsync("/api/clientes", new ClienteRequest("Ana Torres", dni, "987654321"), Json))
            .LeerAsync<ClienteResponse>();

    public static async Task<RepuestoResponse> CrearRepuestoAsync(this HttpClient http, int stock = 3, decimal costo = 80m) =>
        await (await http.PostAsJsonAsync("/api/repuestos",
            new RepuestoRequest("Pantalla Samsung A54", TipoRepuesto.Pantalla, stock, costo), Json))
            .LeerAsync<RepuestoResponse>();

    public static async Task<OrdenResponse> CrearOrdenAsync(this HttpClient http, int clienteId, decimal precio = 250m) =>
        await (await http.PostAsJsonAsync("/api/ordenes",
            new CrearOrdenRequest(clienteId, "Samsung", "A54", null, "Pantalla rota", true, precio, 50m), Json))
            .LeerAsync<OrdenResponse>();
}
