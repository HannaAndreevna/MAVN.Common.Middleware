using System.Security.Claims;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.Sessions.Client;

namespace Falcon.Common.Middleware.Authentication
{
    [PublicAPI]
    public class LykkePrincipal : ILykkePrincipal
    {
        private readonly ClaimsCache _claimsCache = new ClaimsCache();
        private readonly IRequestContext _requestContext;
        private readonly ISessionsServiceClient _sessionsServiceClient;

        public LykkePrincipal(IRequestContext requestContext, ISessionsServiceClient sessionsServiceClient)
        {
            _requestContext = requestContext;
            _sessionsServiceClient = sessionsServiceClient;
        }

        public void InvalidateCache(string token)
        {
            _claimsCache.Invalidate(token);
        }

        public async Task<ClaimsPrincipal> GetCurrentAsync()
        {
            var token = GetToken();

            if (string.IsNullOrWhiteSpace(token))
                return null;

            var result = _claimsCache.Get(token);
            if (result != null)
                return result;

            var session = await _sessionsServiceClient.SessionsApi.GetSessionAsync(token);
            if (session == null)
                return null;

            result = new ClaimsPrincipal(LykkeIdentity.Create(session.ClientId));

            _claimsCache.Set(token, result);

            return result;
        }

        public string GetToken() => _requestContext.SessionToken;
    }
}