using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ModularSys.Core.Interfaces
{
    public interface IAuthService
    {
        // State
        bool IsAuthenticated { get; }
        string? CurrentUser { get; }
        event Action? OnAuthStateChanged;

        // Account actions
        Task<bool> RegisterAsync(string username, string password);
        Task<bool> LoginAsync(string username, string password);
        void Logout();

        // Claims refresh from DB (still async because it hits EF Core)
        Task<bool> RefreshClaimsAsync();
    }
}
