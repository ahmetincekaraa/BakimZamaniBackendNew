namespace KuaforSepeti.Application;

using FluentValidation;
using KuaforSepeti.Application.Mappings;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

/// <summary>
/// Dependency injection extensions for Application layer.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // AutoMapper
        services.AddAutoMapper(typeof(MappingProfile).Assembly);

        // FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}

