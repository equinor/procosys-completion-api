using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;

namespace Equinor.ProCoSys.Completion.WebApi.Authorizations;

public class TokenService : ITokenService
{
    private readonly TokenCredential _tokenCredential;
    public readonly string _scope;

    /// <summary>
    /// Stores the most recently acquired access token for authentication.
    /// This token is used to authorize requests and is refreshed when expired.
    /// </summary>
    public static AccessToken? AccessToken;

    /// <summary>
    /// A <see cref="SemaphoreSlim"/> instance used to synchronize access to the token acquisition process.
    /// </summary>
    /// <remarks>
    /// This semaphore allows only one thread to enter the critical section for acquiring a token at a time.
    /// It helps prevent race conditions and ensures thread safety when multiple threads attempt to acquire a token concurrently.
    /// The initial count is set to 1, allowing one thread to enter, and the maximum count is also set to 1, ensuring exclusive access.
    /// </remarks>
    private static readonly SemaphoreSlim s_tokenLock = new SemaphoreSlim(1, 1);

    public TokenService(TokenCredential tokenCredential, string scope)
    {
        _tokenCredential = tokenCredential;
        _scope = scope;
    }

    /// <summary>
    /// Sets the Authorization header for the HTTP client by retrieving an access token.
    /// Fetches a new token if the existing one is missing or close to expiration.
    /// </summary>
    public async Task<string> AcquireTokenAsync(CancellationToken cancellationToken)
    {
        // First quick check outside the lock to avoid waiting unnecessarily
        // If the token is still valid, we return it immediately
        if (IsAccessTokenExpired())
        {
            await s_tokenLock.WaitAsync(cancellationToken);
            try
            {
                // Double-check inside the lock in case another thread already refreshed the token
                // This prevents redundant refreshes and ensures only one thread refreshes at a time
                if (IsAccessTokenExpired())
                {
                    await RefreshAccessToken(cancellationToken);
                }
            }
            finally
            {
                s_tokenLock.Release();
            }
        }

        // At this point we are guaranteed to have a valid access token
        return AccessToken!.Value.Token;
    }

    private async Task RefreshAccessToken(CancellationToken cancellationToken)
    {
        // Request a new access token using the provided token credential
        var token = await _tokenCredential! // TODO Put Token in configuration
            .GetTokenAsync(new TokenRequestContext(scopes: ["https://StatoilSRM.onmicrosoft.com/Statoil.TI.CommonLibrary.UI/.default"]), cancellationToken);

        if (string.IsNullOrEmpty(token.Token))
        {
            throw new InvalidOperationException("Received an invalid access token.");
        }

        // Store the newly obtained token for reuse
        AccessToken = token;
    }

    /// <summary>
    /// Determines whether the access token is expired or not.
    /// </summary>
    /// <returns>
    /// Returns <c>true</c> if the access token is either not set or has expired; otherwise, <c>false</c>.
    /// The token is considered expired if it is null or if its expiration time is less than or equal to the current UTC time minus one minute.
    /// This allows for a slight buffer to ensure that the token is refreshed before it fully expires.
    /// </returns>
    private static bool IsAccessTokenExpired() => !AccessToken.HasValue || AccessToken.Value.ExpiresOn <= DateTimeOffset.UtcNow.AddMinutes(-1);
}
