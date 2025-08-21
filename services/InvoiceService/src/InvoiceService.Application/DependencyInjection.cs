using Microsoft.Extensions.DependencyInjection;
using InvoiceService.Application.UseCases;

namespace InvoiceService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICreateInvoiceForOrder, CreateInvoiceForOrder>();
        return services;
    }
}
