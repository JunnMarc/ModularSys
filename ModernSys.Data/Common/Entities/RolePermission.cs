using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularSys.Data.Common.Entities
{
    public class RolePermission
    {
        public int RoleId { get; set; }
        public int PermissionId { get; set; }

        public Role? Role { get; set; }
        public Permission? Permission { get; set; }
        //public Department? Department { get; set; }
    }

}
