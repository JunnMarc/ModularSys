using ModularSys.Data.Common.Entities.CRM;
using ModularSys.Data.Common.Entities.Inventory;

namespace ModularSys.Data.Common.Extensions
{
    /// <summary>
    /// Extension methods for SalesOrder to integrate with CRM Customer entity
    /// </summary>
    public static class SalesOrderExtensions
    {
        /// <summary>
        /// Populates SalesOrder customer fields from a Customer entity
        /// </summary>
        public static void PopulateFromCustomer(this SalesOrder salesOrder, Customer customer)
        {
            if (customer == null) return;

            salesOrder.CustomerId = customer.Id;
            salesOrder.CustomerName = customer.ContactName;
            salesOrder.CustomerEmail = customer.Email;
            salesOrder.CustomerPhone = customer.Phone;
            salesOrder.ShippingAddress = customer.Address != null 
                ? $"{customer.Address}, {customer.City}, {customer.State} {customer.PostalCode}, {customer.Country}".Trim(' ', ',')
                : null;
        }

        /// <summary>
        /// Links a SalesOrder to a Customer and populates customer fields
        /// </summary>
        public static void LinkToCustomer(this SalesOrder salesOrder, Customer customer)
        {
            salesOrder.PopulateFromCustomer(customer);
        }

        /// <summary>
        /// Checks if the SalesOrder is linked to a CRM Customer
        /// </summary>
        public static bool HasCustomerLink(this SalesOrder salesOrder)
        {
            return salesOrder.CustomerId.HasValue && salesOrder.CustomerId.Value > 0;
        }

        /// <summary>
        /// Gets the display name for the customer (from entity or legacy field)
        /// </summary>
        public static string GetCustomerDisplayName(this SalesOrder salesOrder)
        {
            if (salesOrder.Customer != null)
            {
                return $"{salesOrder.Customer.ContactName} ({salesOrder.Customer.CompanyName})";
            }
            
            return salesOrder.CustomerName ?? "Unknown Customer";
        }

        /// <summary>
        /// Gets the customer email (from entity or legacy field)
        /// </summary>
        public static string? GetCustomerEmail(this SalesOrder salesOrder)
        {
            return salesOrder.Customer?.Email ?? salesOrder.CustomerEmail;
        }

        /// <summary>
        /// Gets the customer phone (from entity or legacy field)
        /// </summary>
        public static string? GetCustomerPhone(this SalesOrder salesOrder)
        {
            return salesOrder.Customer?.Phone ?? salesOrder.CustomerPhone;
        }
    }
}
