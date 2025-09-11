using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularSys.Core.Interfaces
{
    public interface IClaimsPermissionChecker
    {
        Task<bool> HasPermissionAsync(string permissionName);
        Task<List<string>> GetPermissionsAsync();
    }
}
