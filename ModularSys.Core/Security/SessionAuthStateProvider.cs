using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using ModularSys.Core.Interfaces;

namespace ModularSys.Core.Security
{
    public class SessionAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ISessionStorage _storage;
        private ClaimsPrincipal _cachedUser = new(new ClaimsIdentity());

        private const string KeyClaims = "current_user_claims";

        public SessionAuthStateProvider(ISessionStorage storage)
        {
            _storage = storage;
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var claims = _storage.GetClaims(KeyClaims);
            if (claims != null && claims.Any())
            {
                var identity = new ClaimsIdentity(claims, "Session");
                _cachedUser = new ClaimsPrincipal(identity);
                return Task.FromResult(new AuthenticationState(_cachedUser));
            }

            _cachedUser = new ClaimsPrincipal(new ClaimsIdentity());
            return Task.FromResult(new AuthenticationState(_cachedUser));
        }

        public void NotifyUserAuthentication(IEnumerable<Claim> claims)
        {
            var identity = new ClaimsIdentity(claims, "Session");
            _cachedUser = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_cachedUser)));
        }

        public void NotifyUserLogout()
        {
            _cachedUser = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_cachedUser)));
        }
    }
}
