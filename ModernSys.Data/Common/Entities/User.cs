using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularSys.Data.Common.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public required string Email { get; set; }
        public required int RoleId { get; set; }
        public required int DepartmentId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Role Role { get; set; }
        public Department Department { get; set; }

        //public ICollection<InventoryAdjustment> InventoryAdjustments { get; set; }
        //public ICollection<SalesOrder> SalesOrders { get; set; }
        //public ICollection<ServiceRequest> ServiceRequests { get; set; }
        //public ICollection<AuditTrail> AuditTrails { get; set; }
    }
}
