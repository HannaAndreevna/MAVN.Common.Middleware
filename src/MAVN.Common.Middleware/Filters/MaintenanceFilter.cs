using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using MAVN.Service.MaintenanceMode.Client;
using MAVN.Service.MaintenanceMode.Client.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using StackExchange.Redis;

namespace MAVN.Common.Middleware.Filters
{
    [PublicAPI]
    public class MaintenanceFilter : ActionFilterAttribute
    {
        private const string IsOnMaintenanceCacheKey = "is-on-maintenance";

        private readonly IMaintenanceModeClient _maintenanceModeClient;
        private readonly IDatabase _database;
        private readonly ILog _log;

        [PublicAPI]
        public class MaintenanceResponse
        {
            public string Message { get; set; }
            public int? PlannedDurationInMinutes { get; set; }
            public int? ExpectedRemainingDurationInMinutes { get; set; }
        }

        public MaintenanceFilter(
            IMaintenanceModeClient maintenanceModeClient,
            ILogFactory logFactory,
            IConnectionMultiplexer connectionMultiplexer)
        {
            _maintenanceModeClient = maintenanceModeClient;
            _log = logFactory.CreateLog(this);
            _database = connectionMultiplexer.GetDatabase();
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                if (!context.Controller.GetType().ToString().Contains("IsAliveController"))
                {
                    var value = await _database.StringGetAsync(IsOnMaintenanceCacheKey);
                    MaintenanceModeResponse maintenanceStatus = null;
                    try
                    {
                        if (value.HasValue)
                        {
                            maintenanceStatus = value.ToString().DeserializeJson<MaintenanceModeResponse>();
                        }
                        else
                        {
                            maintenanceStatus = await _maintenanceModeClient.Api.GetActiveMaintenanceDetailsAsync();
                            await _database.StringSetAsync(
                                IsOnMaintenanceCacheKey,
                                maintenanceStatus.ToJson(),
                                TimeSpan.FromMinutes(1));
                        }
                    }
                    catch (Exception e)
                    {
                        _log.Warning("Couldn't fetch maintenance status", e);
                    }

                    if (maintenanceStatus?.IsEnabled ?? false)
                    {
                        ReturnOnMaintenance(context, maintenanceStatus);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
            }

            await next();
        }

        private void ReturnOnMaintenance(ActionExecutingContext actionContext, MaintenanceModeResponse status)
        {
            var expectedRemainingDuration = status.ActualStart.HasValue && status.PlannedDuration.HasValue
                ? (int?)(status.PlannedDuration.Value - (DateTime.UtcNow - status.ActualStart.Value)).TotalMinutes
                : null;
            actionContext.Result = new JsonResult(
                new MaintenanceResponse
                {

                    Message = "Sorry, application is on maintenance. Please try again later.",
                    PlannedDurationInMinutes = (int?)status.PlannedDuration?.TotalMinutes,
                    ExpectedRemainingDurationInMinutes = expectedRemainingDuration,
                })
            {
                StatusCode = 503,
            };
        }
    }
}
