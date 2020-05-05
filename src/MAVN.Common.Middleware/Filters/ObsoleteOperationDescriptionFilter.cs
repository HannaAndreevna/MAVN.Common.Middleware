using System;
using System.Linq;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MAVN.Common.Middleware.Filters
{
    public class ObsoleteOperationDescriptionFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (context.ApiDescription.TryGetMethodInfo(out var methodInfo))
            {
                var attr = methodInfo.GetCustomAttributes(false).FirstOrDefault(x => x is ObsoleteAttribute);
                if (attr != null)
                    operation.Description += (attr as ObsoleteAttribute)?.Message;
            }
        }
    }
}
