using ModularSys.Core.Interfaces;
using Microsoft.Maui.Storage;

namespace ModularSys
{
    public class MauiSessionStorage : ISessionStorage
    {
        public void Set(string key, string value) => Preferences.Set(key, value);
        public string? Get(string key) => Preferences.Get(key, null);
        public void Remove(string key) => Preferences.Remove(key);
    }
}
