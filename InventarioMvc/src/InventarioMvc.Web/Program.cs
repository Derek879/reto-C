using System.Globalization;
using InventarioMvc.Web.Data;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var cadena = builder.Configuration.GetConnectionString("SqlServer")
    ?? throw new InvalidOperationException("Falta ConnectionStrings:SqlServer.");
builder.Services.AddSingleton<IConexionSql>(new ConexionSql(cadena));
builder.Services.AddScoped<ProductoRepositorio>();
builder.Services.AddScoped<VentaRepositorio>();
builder.Services.AddScoped<ReporteRepositorio>();
builder.Services.AddSingleton(TimeProvider.System);

var app = builder.Build();

if (app.Configuration.GetValue<bool>("Database:InicializarAlIniciar"))
    await InicializadorBd.EjecutarAsync(cadena, Path.Combine(AppContext.BaseDirectory, "database"));

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Montos en soles y fechas en formato peruano
var peru = new CultureInfo("es-PE");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(peru),
    SupportedCultures = [peru],
    SupportedUICultures = [peru],
});

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public partial class Program;
