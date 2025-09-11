using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ModularSys.Core.Security;

public interface IPermissionChecker
{
    Task<bool> HasPermissionAsync(int userId, string permissionName);
    Task<List<string>> GetPermissionsAsync(int userId);
}



