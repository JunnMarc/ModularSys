using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularSys.Core.Interfaces
{
    public interface ISessionStorage
    {
        void Set(string key, string value);
        string? Get(string key);
        void Remove(string key);
    }
}

