﻿using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests;

internal class IntegrationTestAuthHandler : AuthenticationHandler<IntegrationTestAuthOptions>
{
    public static string TestAuthenticationScheme = "PCS_Api_IntTest";

    private enum AuthType
    {
        Application,
        Delegated
    }

    public IntegrationTestAuthHandler(
        IOptionsMonitor<IntegrationTestAuthOptions> options, 
        ILoggerFactory logger, 
        UrlEncoder encoder) 
        : base(options, logger, encoder)
    {
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            var claims = await GatherTestUserClaimsAsync();
            var testIdentity = new ClaimsIdentity(claims, TestAuthenticationScheme);
            var testUser = new ClaimsPrincipal(testIdentity);
            var ticket = new AuthenticationTicket(testUser, new AuthenticationProperties(), TestAuthenticationScheme);
            // Don't think there is any scenario we want to return 401, as if headers is set, the user is requested.
            return AuthenticateResult.Success(ticket);
        }
        catch (Exception ex)
        {
            return AuthenticateResult.Fail(ex);
        }
    }

    private Task<List<Claim>> GatherTestUserClaimsAsync()
    {
        var authorizationHeader = Request.Headers["Authorization"];
        var tokens = authorizationHeader.ToString().Split(' ');
        if (tokens is null || tokens.Length <= 1)
        {
            throw new Exception("[Authorization] header missing");
        }

        TestProfile profile;
        var tokenPart = tokens[1];
        try
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(tokenPart));
            profile = JsonSerializer.Deserialize<TestProfile>(decoded, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (profile is null)
            {
                throw new Exception("Error Deserialize token");
            }
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Unable to extract test auth token from [Authorization] header. See inner exception for details.",
                ex);
        }

        var authType = profile.IsAppToken ? AuthType.Application : AuthType.Delegated;

        var claims = new List<Claim> {new(ClaimsExtensions.Oid, profile.Oid)};

        switch (authType)
        {
            case AuthType.Delegated:
                claims.Add(new Claim(ClaimTypes.GivenName, profile.FirstName));
                claims.Add(new Claim(ClaimTypes.Surname, profile.LastName));
                break;
            case AuthType.Application:
                break;
        }

        if (profile.AppRoles is not null)
        {
            foreach (var role in profile.AppRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        return Task.FromResult(claims);
    }
}
