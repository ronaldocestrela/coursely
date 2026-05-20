using System.Reflection;
using Application.Common.Behaviors;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddValidatorsFromAssembly(assembly);

        services.AddSingleton(_ =>
            new MapperConfiguration(
                cfg => cfg.AddMaps(assembly),
                NullLoggerFactory.Instance));

        services.AddSingleton<IMapper>(sp =>
            sp.GetRequiredService<MapperConfiguration>().CreateMapper());

        return services;
    }
}
