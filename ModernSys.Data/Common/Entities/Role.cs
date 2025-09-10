using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularSys.Data.Common.Entities
{
    public class Role
    {
        public int RoleId { get; set; }
        public required string RoleName { get; set; }
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    }
}
