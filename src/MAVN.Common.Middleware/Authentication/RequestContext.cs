using JetBrains.Annotations;
using MAVN.Service.Sessions.Client;
using Microsoft.AspNetCore.Http;

namespace MAVN.Common.Middleware.Authentication
{
    [PublicAPI]
    public class RequestContext : IRequestContext
    {
        private readonly HttpContext _httpContext;
        private readonly ISessionsServiceClient _sessionsServiceClient;

        public RequestContext(IHttpContextAccessor httpContextAccessor,
            ISessionsServiceClient sessionsServiceClient)
        {
            _sessionsServiceClient = sessionsServiceClient;
            _httpContext = httpContextAccessor.HttpContext;
        }

        public string SessionToken => _httpContext.GetLykkeToken();

        public string UserId => GetUserId();

        private string GetUserId()
        {
            var token = _httpContext.GetLykkeToken();

            if (string.IsNullOrWhiteSpace(token))
                return null;

            var session = _sessionsServiceClient.SessionsApi.GetSessionAsync(token).GetAwaiter().GetResult();

            return session?.ClientId;
        }
    }
}