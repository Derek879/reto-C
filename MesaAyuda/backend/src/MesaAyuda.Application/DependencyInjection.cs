using FluentValidation;
using MesaAyuda.Application.Tickets;
using Microsoft.Extensions.DependencyInjection;

namespace MesaAyuda.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CrearTicketValidador>();
        services.AddScoped<TicketService>();
        return services;
    }
}
