using Microsoft.AspNetCore.Components.Authorization;
using ModularSys.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularSys.Core.Security
{
    public class ClaimsPermissionChecker : IClaimsPermissionChecker
    {
        private readonly AuthenticationStateProvider _authStateProvider;

        public ClaimsPermissionChecker(AuthenticationStateProvider authStateProvider)
        {
            _authStateProvider = authStateProvider;
        }

        public async Task<bool> HasPermissionAsync(string permissionName)
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            return user.Identity?.IsAuthenticated == true &&
                   user.HasClaim("Permission", permissionName);
        }

        public async Task<List<string>> GetPermissionsAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            return user.Identity?.IsAuthenticated == true
                ? user.Claims.Where(c => c.Type == "Permission").Select(c => c.Value).ToList()
                : new List<string>();
        }
    }
}
