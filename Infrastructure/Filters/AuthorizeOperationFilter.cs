using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ShoppingMarket.Infrastructure.Filters;

public class AuthorizeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAuthorize = context.MethodInfo
            .GetCustomAttributes(true)
            .OfType<AuthorizeAttribute>()
            .Any();

        var endpointMetadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;

        var requiresAuth = endpointMetadata
            .OfType<IAuthorizeData>()
            .Any();

        if (!hasAuthorize && !requiresAuth)
        {
            operation.Security = [];
        }
    }
}