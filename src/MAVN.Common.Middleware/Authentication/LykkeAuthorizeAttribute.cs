using System;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Falcon.Common.Middleware.Authentication
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    [PublicAPI]
    public class LykkeAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private ILog _log;
        private ILykkePrincipal _lykkePrincipal;

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var logFactory = context.HttpContext.RequestServices.GetService<ILogFactory>();
            _log = logFactory.CreateLog(this);
            _lykkePrincipal = context.HttpContext.RequestServices.GetService<ILykkePrincipal>();

            try
            {
                var principal = await _lykkePrincipal.GetCurrentAsync();
                if (principal == null)
                {
                    context.Result = new UnauthorizedObjectResult(new { Error = "Not authenticated" });
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, context: _lykkePrincipal.GetToken());
            }
        }
    }
}