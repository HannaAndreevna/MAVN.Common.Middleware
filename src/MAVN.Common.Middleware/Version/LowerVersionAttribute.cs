using System.Collections.Generic;
using System.Linq;
using System.Net;
using Common;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MAVN.Common.Middleware.Version
{
    [PublicAPI]
    public class LowerVersionAttribute : ActionFilterAttribute
    {
        private string[] DevicesArray { get; set; }

        private string _device;

        public string Devices
        {
            get => _device;
            set
            {
                _device = value;
                DevicesArray = value.Split(',').Select(x => x.Trim().ToLower()).ToArray();
            }
        }

        public int LowerVersion { get; set; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            string device = string.Empty;
            bool notSupported = false;
            try
            {
                //e.g. "User-Agent: DeviceType=iPhone;AppVersion=112"
                var userAgent = GetUserAgent(context.HttpContext.Request);

                if (IsValidUserAgent(userAgent))
                {
                    var parametersDict = ParseUserAgent(userAgent);

                    if (DevicesArray.Contains(parametersDict[UserAgentVariablesLowercase.DeviceType]))
                    {
                        if (parametersDict[UserAgentVariablesLowercase.AppVersion].ParseAnyDouble() < LowerVersion)
                        {
                            device = parametersDict[UserAgentVariablesLowercase.DeviceType];
                            notSupported = true;
                        }
                    }
                }
            }
            catch
            {
                notSupported = true;
            }
            finally
            {
                if (notSupported)
                    ReturnNotSupported(context, device);
            }
        }

        private string GetUserAgent(HttpRequest request)
        {
            return request.Headers["User-Agent"].ToString().ToLower();
        }

        private bool IsValidUserAgent(string userAgent)
        {
            return userAgent.Contains(UserAgentVariablesLowercase.DeviceType) && userAgent.Contains(UserAgentVariablesLowercase.AppVersion);
        }

        private void ReturnNotSupported(ActionExecutingContext actionContext, string device)
        {
            string msg = device == DeviceTypesLowercase.Android
                ? "Your app version is not supported anymore. Please update it from Google Play and continue enjoying the service."
                : "Your app version is not supported anymore. Please update it and continue enjoying the service.";
            var response = new { Message = msg };
            actionContext.Result = new JsonResult(response) { StatusCode = (int)HttpStatusCode.ExpectationFailed };
        }

        private IDictionary<string, string> ParseUserAgent(string userAgent)
        {
            if (!string.IsNullOrEmpty(userAgent))
                return userAgent.Split(';').Select(parameter => parameter.Split('='))
                    .Where(x => x.Length == 2)
                    .GroupBy(x => x[0])
                    .ToDictionary(x => x.Key, x => x.First()[1]);
            return new Dictionary<string, string>();
        }
    }
}
