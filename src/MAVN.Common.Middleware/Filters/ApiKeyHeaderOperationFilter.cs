using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MAVN.Common.Middleware.Filters
{
    [PublicAPI]
    public class ApiKeyHeaderOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var filterPipeline = context.ApiDescription.ActionDescriptor.FilterDescriptors;
            var isActionFilter = filterPipeline.Select(filterInfo => filterInfo.Filter).Any(filter => filter is IAsyncAuthorizationFilter);
            var allowAnonymous = filterPipeline.Select(filterInfo => filterInfo.Filter).Any(filter => filter is IAllowAnonymousFilter);
            if (isActionFilter && !allowAnonymous)
            {
                if (operation.Parameters == null)
                    operation.Parameters = new List<IParameter>();

                operation.Parameters.Add(
                    new NonBodyParameter
                    {
                        Name = "Authorization",
                        In = "header",
                        Description = "Access token",
                        Required = true,
                        Type = "string"
                    });
            }
        }
    }
}
