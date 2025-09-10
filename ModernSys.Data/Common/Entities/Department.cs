using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularSys.Data.Common.Entities
{
    public class Department
    {
        public int DepartmentId { get; set; }
        public required string DepartmentName { get; set; }
        public string DepartmentDesc { get; set; } = null!;

        public ICollection<User> Users  = new List<User>();
    }
}
