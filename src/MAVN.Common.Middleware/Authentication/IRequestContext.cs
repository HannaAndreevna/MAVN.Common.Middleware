using JetBrains.Annotations;

namespace Falcon.Common.Middleware.Authentication
{
    [PublicAPI]
    public interface IRequestContext
    {
        string SessionToken { get; }
        string UserId { get; }
    }
}