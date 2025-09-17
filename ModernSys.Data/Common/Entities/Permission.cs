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
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = "General";
        public string Icon { get; set; } = "Lock";
        public int DisplayOrder { get; set; } = 0;
        public bool IsSystemPermission { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
