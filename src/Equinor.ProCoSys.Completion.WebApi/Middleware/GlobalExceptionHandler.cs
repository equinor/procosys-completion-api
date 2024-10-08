﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.WebApi.Middleware;

public class GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Call the next delegate/middleware in the pipeline
            await next(context);
        }
        catch (UnauthorizedAccessException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            context.Response.ContentType = "application/text";
            await context.Response.WriteAsync("Unauthorized!");
        }
        catch (BadRequestException bex)
        {
            var errors = new Dictionary<string, string[]>
            {
                { "guid", [bex.Message] }
            };
            await context.WriteBadRequestAsync(errors, logger);
        }
        catch (EntityNotFoundException ne)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            context.Response.ContentType = "application/text";
            logger.LogDebug(ne.Message);
            await context.Response.WriteAsync(ne.Message);
        }
        catch (FluentValidation.ValidationException ve)
        {
            if (AnyNotFoundValidationErrors(ve.Errors, out var notFoundMessage))
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                context.Response.ContentType = "application/text";
                await context.Response.WriteAsync(notFoundMessage);
            }
            else
            {
                var errors = new Dictionary<string, string[]>();
                foreach (var error in ve.Errors)
                {
                    if (!errors.ContainsKey(error.PropertyName))
                    {
                        errors.Add(error.PropertyName, [error.ErrorMessage]);
                    }
                    else
                    {
                        var errorsForProperty = errors[error.PropertyName].ToList();
                        errorsForProperty.Add(error.ErrorMessage);
                        errors[error.PropertyName] = errorsForProperty.ToArray();
                    }
                }

                await context.WriteBadRequestAsync(errors, logger);
            }
        }
        catch (ConcurrencyException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Conflict;
            context.Response.ContentType = "application/text";
            const string Message = "Data store operation failed. Data may have been modified or deleted since entities were loaded.";
            logger.LogDebug(Message);
            await context.Response.WriteAsync(Message);
        }
        catch (TaskCanceledException)
        {
            context.Response.StatusCode = 499; //Client Closed Request;
            context.Response.ContentType = "application/text";
            const string Message = "Request canceled";
            logger.LogInformation(Message);
            await context.Response.WriteAsync(Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception occurred");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/text";
            await context.Response.WriteAsync("Something went wrong!");
        }
    }

    private bool AnyNotFoundValidationErrors(IEnumerable<ValidationFailure> errors, out string firstNotFoundErrorMessage)
    {
        var notFoundError = errors.FirstOrDefault(
            e => e.CustomState is not null && 
                 e.CustomState.GetType() == typeof(EntityNotFoundException));

        if (notFoundError is null)
        {
            firstNotFoundErrorMessage = string.Empty;
            return false;
        }

        firstNotFoundErrorMessage = notFoundError.ErrorMessage;
        return true;
    }
}
