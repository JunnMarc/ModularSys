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
        string? CurrentUserRole { get; }
        int? CurrentUserCustomerId { get; }
        event Action? OnAuthStateChanged;

        // Account actions
        Task<bool> RegisterAsync(string username, string password);
        Task<(bool Success, string Message)> RegisterCustomerAsync(string username, string password, string firstName, string lastName, string email);
        Task<bool> LoginAsync(string username, string password);
        void Logout();

        // Claims refresh from DB (still async because it hits EF Core)
        Task<bool> RefreshClaimsAsync();

        // Helper for API usage (or move to separate service)
        string HashPassword(string password);
    }
}
