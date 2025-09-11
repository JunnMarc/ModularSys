using System.Collections.Generic;
using System.Security.Claims;

namespace ModularSys.Core.Interfaces
{
    public interface ISessionStorage
    {
        // Basic key/value storage
        void Set(string key, string value);
        string? Get(string key);
        void Remove(string key);

        // Claims storage
        void SetClaims(string key, List<Claim> claims);
        List<Claim>? GetClaims(string key);
    }
}
