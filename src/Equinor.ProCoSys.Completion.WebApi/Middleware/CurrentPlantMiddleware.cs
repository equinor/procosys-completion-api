﻿using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.WebApi.Misc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.WebApi.Middleware;

public class CurrentPlantMiddleware
{
    public const string PlantHeader = "x-plant";

    private readonly RequestDelegate _next;

    public CurrentPlantMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(
        HttpContext context,
        IHttpContextAccessor httpContextAccessor,
        IPlantSetter plantSetter,
        ILogger<CurrentPlantMiddleware> logger)
    {
        logger.LogDebug("----- {MiddlewareName} start", GetType().Name);
        var headers = httpContextAccessor.HttpContext?.Request.Headers;
        if (headers is null)
        {
            throw new Exception("Could not determine request headers");
        }

        var plant = headers.GetPlant();
        if(plant is not null)
        {
            plantSetter.SetPlant(plant);
            logger.LogDebug("----- {MiddlewareName} complete setting plant {Plant}", GetType().Name, plant);
        }
        else
        {
            logger.LogDebug("----- {MiddlewareName} complete. No plant header set", GetType().Name);
        }

        // Call the next delegate/middleware in the pipeline
        await _next(context);
    }
}
