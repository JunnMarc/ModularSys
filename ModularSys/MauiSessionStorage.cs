using Microsoft.Maui.Storage;
using ModularSys.Core.Interfaces;
using System.Security.Claims;

namespace ModularSys
{
    public class MauiSessionStorage : ISessionStorage
    {
        private const string ClaimsSeparator = "||";
        private const string FieldSeparator = "|";

        public void Set(string key, string value)
            => Preferences.Set(key, value);

        public string? Get(string key)
            => Preferences.Get(key, null);

        public void Remove(string key)
            => Preferences.Remove(key);

        public void SetClaims(string key, List<Claim> claims)
        {
            var flat = string.Join(ClaimsSeparator, claims.Select(c => $"{c.Type}{FieldSeparator}{c.Value}"));
            Preferences.Set(key, flat);
        }

        public List<Claim>? GetClaims(string key)
        {
            var flat = Preferences.Get(key, null);
            if (string.IsNullOrEmpty(flat))
                return null;

            var claims = new List<Claim>();
            var parts = flat.Split(ClaimsSeparator, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                var typeValue = part.Split(FieldSeparator, 2);
                if (typeValue.Length == 2)
                    claims.Add(new Claim(typeValue[0], typeValue[1]));
            }

            return claims;
        }
    }
}
