using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularSys.Data.Common.Entities
{
    public class Permission
    {
        public int PermissionId { get; set; }
        public required string PermissionName { get; set; }
        public string Description { get; set; } = null!;

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
