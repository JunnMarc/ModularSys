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
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public string? ContactNumber { get; set; }
        public required int RoleId { get; set; }
        public required int DepartmentId { get; set; }
        public DateTime CreatedAt { get; set; }

        // Soft Delete Properties
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public string? CreatedBy { get; set; }

        public Role Role { get; set; }
        public Department Department { get; set; }

        [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
        public User() { }

        [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
        public User(string username, string firstName, string lastName, string email, int departmentId)
        {
            Username = username;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            DepartmentId = departmentId;
        }

        //public ICollection<InventoryAdjustment> InventoryAdjustments { get; set; }
        //public ICollection<SalesOrder> SalesOrders { get; set; }
        //public ICollection<ServiceRequest> ServiceRequests { get; set; }
        //public ICollection<AuditTrail> AuditTrails { get; set; }
    }
}
