using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.Incoming;
using OrderService.Application.UseCases;

namespace OrderService.Application;   // <-- BUNA DİKKAT ET

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IValidator<TrendyolWebhookDto>, TrendyolWebhookDtoValidator>();
        services.AddScoped<ICreateOrderFromTrendyol, CreateOrderFromTrendyol>();
        return services;
    }
}
