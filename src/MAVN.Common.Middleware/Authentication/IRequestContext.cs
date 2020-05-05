using JetBrains.Annotations;

namespace MAVN.Common.Middleware.Authentication
{
    [PublicAPI]
    public interface IRequestContext
    {
        string SessionToken { get; }
        string UserId { get; }
    }
}