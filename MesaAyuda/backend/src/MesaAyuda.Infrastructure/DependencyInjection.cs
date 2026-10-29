using MesaAyuda.Application.Tickets;
using MesaAyuda.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MesaAyuda.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<MesaAyudaDbContext>(o => o.UseSqlServer(config.GetConnectionString("SqlServer")));
        services.AddScoped<ITicketRepositorio, TicketRepositorio>();
        return services;
    }
}
