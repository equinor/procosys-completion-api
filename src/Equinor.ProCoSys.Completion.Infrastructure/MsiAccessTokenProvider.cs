﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;

namespace Equinor.ProCoSys.Completion.Infrastructure;
public static class MsiAccessTokenProvider
{
    public static AccessToken? AccessToken { get; private set; }

    public static async Task<string> GetAccessTokenAsync(TokenCredential credential)
    {
        if (AccessToken.HasValue && AccessToken.Value.ExpiresOn > DateTimeOffset.UtcNow)
            return AccessToken.Value.Token;

        AccessToken = await credential.GetTokenAsync(
            new TokenRequestContext(scopes: ["https://database.windows.net/.default"]),
            CancellationToken.None
        );

        return AccessToken.Value.Token;
    }
}