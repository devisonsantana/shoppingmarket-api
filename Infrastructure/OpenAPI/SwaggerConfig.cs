using System.Reflection;
using Microsoft.OpenApi;
using ShoppingMarket.Infrastructure.Filters;
namespace ShoppingMarket.Infrastructure.OpenAPI;

public static class SwaggerConfig
{
    public static IServiceCollection AddSwaggerWithAuth(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new()
            {
                Version = "v1",
                Title = "Shopping Market API",
                Description = "An ASP.NET Core Minimal Web API for Shopping Market",
            });

            // using System.Reflection;
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Insert a valid WebToken"
            });

            options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference("Bearer", doc)] = [] });
            
            options.OperationFilter<AuthorizeOperationFilter>();
        });

        return services;
    }
}