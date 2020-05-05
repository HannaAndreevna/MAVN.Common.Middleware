using System.Security.Claims;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace MAVN.Common.Middleware.Authentication
{
    [PublicAPI]
    public interface ILykkePrincipal
    {
        Task<ClaimsPrincipal> GetCurrentAsync();
        void InvalidateCache(string token);
        string GetToken();
    }
}