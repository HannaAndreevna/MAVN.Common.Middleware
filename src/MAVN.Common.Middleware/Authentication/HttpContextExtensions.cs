using Microsoft.AspNetCore.Http;

namespace Falcon.Common.Middleware.Authentication
{
    public static class HttpContextExtensions
    {
        public static string GetLykkeToken(this HttpContext context)
        {
            var str = context.Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(str))
                return null;

            var strArray = str.Split(' ');
            if (strArray.Length != 2 || strArray[0] != "Bearer")
                return null;

            return strArray[1];
        }
    }
}