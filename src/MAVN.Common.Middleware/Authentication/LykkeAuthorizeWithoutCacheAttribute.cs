using System;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using MAVN.Service.Sessions.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace MAVN.Common.Middleware.Authentication
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    [PublicAPI]
    public class LykkeAuthorizeWithoutCacheAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private ISessionsServiceClient _sessionsServiceClient;
        private ILog _log;
        private IRequestContext _requestContext;

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            _sessionsServiceClient = context.HttpContext.RequestServices.GetService<ISessionsServiceClient>();
            var logFactory = context.HttpContext.RequestServices.GetService<ILogFactory>();
            _log = logFactory.CreateLog(this);
            _requestContext = context.HttpContext.RequestServices.GetService<IRequestContext>();

            try
            {
                if (string.IsNullOrEmpty(_requestContext.SessionToken))
                {
                    context.Result = new UnauthorizedObjectResult(new { Error = "Not authenticated" });
                }
                else
                {
                    var clientSession = await _sessionsServiceClient.SessionsApi.GetSessionAsync(_requestContext.SessionToken);
                    if (clientSession == null)
                    {
                        context.Result = new UnauthorizedObjectResult(new { Error = "Not authenticated" });
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, context: _requestContext.SessionToken);
            }
        }
    }
}