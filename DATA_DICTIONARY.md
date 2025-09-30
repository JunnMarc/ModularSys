# ModularSys Database - Data Dictionary

## How to Convert to DOCX:
1. Open this file in Microsoft Word
2. Select all tables
3. Use "Convert Text to Table" feature
4. Or copy-paste into Word and format as tables

---

## 1. Users Table

| Field Name | Data Type | Length | Description |
|------------|-----------|--------|-------------|
| Id (PK) | Int - AI | 9 | User's unique identifier (Primary Key, Auto-Increment) |
| Username | Varchar | 50 | User's login username (Unique) |
| PasswordHash | Varchar | 255 | Encrypted password using BCrypt |
| Email | Varchar | 100 | User's email address |
| FirstName | Varchar | 50 | User's first name |
| LastName | Varchar | 50 | User's last name |
| RoleId (FK) | Int | 9 | Foreign Key to Roles table |
| DepartmentId (FK) | Int | 9 | Foreign Key to Departments table |
| CreatedAt | DateTime | - | Record creation timestamp |
| CreatedBy | Varchar | 50 | Username who created the record |
| UpdatedAt | DateTime | - | Last update timestamp |
| UpdatedBy | Varchar | 50 | Username who last updated the record |
| IsDeleted | Bit | 1 | Soft delete flag (0=Active, 1=Deleted) |
| DeletedAt | DateTime | - | Deletion timestamp |
| DeletedBy | Varchar | 50 | Username who deleted the record |

---

## 2. Roles Table

| Field Name | Data Type | Length | Description |
|------------|-----------|--------|-------------|
| RoleId (PK) | Int - AI | 9 | Role's unique identifier (Primary Key, Auto-Increment) |
| RoleName | Varchar | 50 | Role name (e.g., Admin, Manager, Staff) - Unique |
| Description | Varchar | 255 | Role description and responsibilities |
| CreatedAt | DateTime | - | Record creation timestamp |
| CreatedBy | Varchar | 50 | Username who created the role |
| UpdatedAt | DateTime | - | Last update timestamp |
| UpdatedBy | Varchar | 50 | Username who last updated the role |

---

## 3. Permissions Table

| Field Name | Data Type | Length | Description |
|------------|-----------|--------|-------------|
| PermissionId (PK) | Int - AI | 9 | Permission's unique identifier (Primary Key, Auto-Increment) |
| PermissionName | Varchar | 100 | Permission name (e.g., Users.View, Products.Create) - Unique |
| Description | Varchar | 255 | Permission description |
| Category | Varchar | 50 | Permission category (Users, Products, Orders, etc.) |
| Icon | Varchar | 50 | Icon name for UI display |
| DisplayOrder | Int | 9 | Display order in UI |
| IsSystemPermission | Bit | 1 | System permission flag (1=System, 0=Custom) |
| CreatedAt | DateTime | - | Record creation timestamp |
| CreatedBy | Varchar | 50 | Username who created the permission |

---

## 4. RolePermissions Table (Junction Table)

| Field Name | Data Type | Length | Description |
|------------|-----------|--------|-------------|
| RoleId (PK, FK) | Int | 9 | Foreign Key to Roles table (Composite Primary Key) |
| PermissionId (PK, FK) | Int | 9 | Foreign Key to Permissions table (Composite Primary Key) |
| GrantedAt | DateTime | - | Timestamp when permission was granted |
| GrantedBy | Varchar | 50 | Username who granted the permission |

---

## 5. Departments Table

| Field Name | Data Type | Length | Description |
|------------|-----------|--------|-------------|
| DepartmentId (PK) | Int - AI | 9 | Department's unique identifier (Primary Key, Auto-Increment) |
| DepartmentName | Varchar | 100 | Department name (e.g., Sales, IT, Finance) - Unique |
| Description | Varchar | 255 | Department description |
| CreatedAt | DateTime | - | Record creation timestamp |
| CreatedBy | Varchar | 50 | Username who created the department |
| UpdatedAt | DateTime | - | Last update timestamp |
| UpdatedBy | Varchar | 50 | Username who last updated the department |

---

## 6. Categories Table

| Field Name | Data Type | Length | Description |
|------------|-----------|--------|-------------|
| CategoryId (PK) | Int - AI | 9 | Category's unique identifier (Primary Key, Auto-Increment) |
| Name | Varchar | 100 | Category name (e.g., Electronics, Accessories) - Unique |
| Description | Varchar | 500 | Category description |
| CreatedAt | DateTime | - | Record creation timestamp |
| CreatedBy | Varchar | 50 | Username who created the category |
| UpdatedAt | DateTime | - | Last update timestamp |
| UpdatedBy | Varchar | 50 | Username who last updated the category |
| IsDeleted | Bit | 1 | Soft delete flag (0=Active, 1=Deleted) |
| DeletedAt | DateTime | - | Deletion timestamp |
| DeletedBy | Varchar | 50 | Username who deleted the record |

---

## 7. Products Table

| Field Name | Data Type | Length | Description |
|------------|-----------|--------|-------------|
| ProductId (PK) | Int - AI | 9 | Product's unique identifier (Primary Key, Auto-Increment) |
| Name | Varchar | 200 | Product name |
| SKU | Varchar | 50 | Stock Keeping Unit - Unique product code |
| Description | Text | - | Detailed product description |
| CategoryId (FK) | Int | 9 | Foreign Key to Categories table |
| CostPrice | Decimal | 18,2 | Product cost price (purchase price) |
| SellingPrice | Decimal | 18,2 | Product selling price (retail price) |
| QuantityOnHand | Int | 9 | Current stock quantity |
| ReorderLevel | Int | 9 | Minimum stock level before reorder alert |
| CreatedAt | DateTime | - | Record creation timestamp |
| CreatedBy | Varchar | 50 | Username who created the product |
| UpdatedAt | DateTime | - | Last update timestamp |
| UpdatedBy | Varchar | 50 | Username who last updated the product |
| IsDeleted | Bit | 1 | Soft delete flag (0=Active, 1=Deleted) |
| DeletedAt | DateTime | - | Deletion timestamp |
| DeletedBy | Varchar | 50 | Username who deleted the record |

---

## 8. SalesOrders Table

| Field Name | Data Type | Length | Description |
|------------|-----------|--------|-------------|
| SalesOrderId (PK) | Int - AI | 9 | Sales order's unique identifier (Primary Key, Auto-Increment) |
| OrderNumber | Varchar | 50 | Unique order number (e.g., SO-20250101-001) |
| CustomerName | Varchar | 200 | Customer's full name |
| CustomerEmail | Varchar | 100 | Customer's email address |
| CustomerPhone | Varchar | 20 | Customer's phone number |
| SubTotal | Decimal | 18,2 | Order subtotal (before discounts and tax) |
| DiscountAmount | Decimal | 18,2 | Total discount amount applied |
| TaxAmount | Decimal | 18,2 | Total tax amount |
| ShippingCost | Decimal | 18,2 | Shipping/delivery cost |
| TotalAmount | Decimal | 18,2 | Final total amount (SubTotal - Discount + Tax + Shipping) |
| Status | Varchar | 20 | Order status (Pending, Completed, Cancelled) |
| OrderDate | DateTime | - | Order creation date |
| CreatedAt | DateTime | - | Record creation timestamp |
| CreatedBy | Varchar | 50 | Username who created the order |
| UpdatedAt | DateTime | - | Last update timestamp |
| UpdatedBy | Varchar | 50 | Username who last updated the order |
| IsDeleted | Bit | 1 | Soft delete flag (0=Active, 1=Deleted) |
| DeletedAt | DateTime | - | Deletion timestamp |
| DeletedBy | Varchar | 50 | Username who deleted the record |

---

## 9. SalesOrderLines Table

| Field Name | Data Type | Length | Description |
|------------|-----------|--------|-------------|
| SalesOrderLineId (PK) | Int - AI | 9 | Order line's unique identifier (Primary Key, Auto-Increment) |
| SalesOrderId (FK) | Int | 9 | Foreign Key to SalesOrders table |
| ProductId (FK) | Int | 9 | Foreign Key to Products table |
| Quantity | Int | 9 | Quantity of product ordered |
| UnitPrice | Decimal | 18,2 | Price per unit at time of order |
| DiscountAmount | Decimal | 18,2 | Discount applied to this line |
| LineTotal | Decimal | 18,2 | Line total (Quantity × UnitPrice - Discount) |
| CreatedAt | DateTime | - | Record creation timestamp |

---

## 10. PurchaseOrders Table

| Field Name | Data Type | Length | Description |
|------------|-----------|--------|-------------|
| PurchaseOrderId (PK) | Int - AI | 9 | Purchase order's unique identifier (Primary Key, Auto-Increment) |
| OrderNumber | Varchar | 50 | Unique order number (e.g., PO-20250101-001) |
| SupplierName | Varchar | 200 | Supplier's company name |
| SupplierEmail | Varchar | 100 | Supplier's email address |
| SupplierPhone | Varchar | 20 | Supplier's phone number |
| SubTotal | Decimal | 18,2 | Order subtotal |
| TaxAmount | Decimal | 18,2 | Total tax amount |
| ShippingCost | Decimal | 18,2 | Shipping/delivery cost |
| TotalAmount | Decimal | 18,2 | Final total amount (SubTotal + Tax + Shipping) |
| Status | Varchar | 20 | Order status (Pending, Received, Cancelled) |
| OrderDate | DateTime | - | Order creation date |
| CreatedAt | DateTime | - | Record creation timestamp |
| CreatedBy | Varchar | 50 | Username who created the order |
| UpdatedAt | DateTime | - | Last update timestamp |
| UpdatedBy | Varchar | 50 | Username who last updated the order |
| IsDeleted | Bit | 1 | Soft delete flag (0=Active, 1=Deleted) |
| DeletedAt | DateTime | - | Deletion timestamp |
| DeletedBy | Varchar | 50 | Username who deleted the record |

---

## 11. PurchaseOrderLines Table

| Field Name | Data Type | Length | Description |
|------------|-----------|--------|-------------|
| PurchaseOrderLineId (PK) | Int - AI | 9 | Order line's unique identifier (Primary Key, Auto-Increment) |
| PurchaseOrderId (FK) | Int | 9 | Foreign Key to PurchaseOrders table |
| ProductId (FK) | Int | 9 | Foreign Key to Products table |
| Quantity | Int | 9 | Quantity of product ordered |
| UnitCost | Decimal | 18,2 | Cost per unit at time of order |
| LineTotal | Decimal | 18,2 | Line total (Quantity × UnitCost) |
| CreatedAt | DateTime | - | Record creation timestamp |

---

## 12. InventoryTransactions Table

| Field Name | Data Type | Length | Description |
|------------|-----------|--------|-------------|
| InventoryTransactionId (PK) | Int - AI | 9 | Transaction's unique identifier (Primary Key, Auto-Increment) |
| TransactionNumber | Varchar | 50 | Unique transaction number (e.g., TXN-20250101-ABC123) |
| ProductId (FK) | Int | 9 | Foreign Key to Products table |
| TransactionDate | DateTime | - | Transaction date and time |
| TransactionType | Varchar | 20 | Type (Sale, Purchase, Adjustment, Return, Transfer) |
| QuantityBefore | Int | 9 | Stock quantity before transaction |
| QuantityChange | Int | 9 | Quantity change (negative for outbound, positive for inbound) |
| QuantityAfter | Int | 9 | Stock quantity after transaction |
| UnitCost | Decimal | 18,2 | Cost per unit at time of transaction |
| Amount | Decimal | 18,2 | Total monetary value of transaction |
| Reason | Varchar | 255 | Reason for adjustment/transfer |
| BatchNumber | Varchar | 50 | Batch or lot number |
| Reference | Varchar | 100 | Reference to related order/document |
| ReferenceId | Int | 9 | ID of related order |
| Notes | Text | - | Additional notes |
| CreatedAt | DateTime | - | Record creation timestamp |
| CreatedBy | Varchar | 50 | Username who created the transaction |
| UpdatedAt | DateTime | - | Last update timestamp |
| UpdatedBy | Varchar | 50 | Username who last updated the transaction |
| IsDeleted | Bit | 1 | Soft delete flag (0=Active, 1=Deleted) |
| DeletedAt | DateTime | - | Deletion timestamp |
| DeletedBy | Varchar | 50 | Username who deleted the record |

---

## 13. RevenueTransactions Table

| Field Name | Data Type | Length | Description |
|------------|-----------|--------|-------------|
| RevenueTransactionId (PK) | Int - AI | 9 | Revenue transaction's unique identifier (Primary Key, Auto-Increment) |
| Amount | Decimal | 18,2 | Transaction amount (positive for income, negative for expense) |
| Source | Varchar | 50 | Revenue source (Sale, Purchase, Adjustment) |
| Reference | Varchar | 100 | Reference to related order/document |
| TransactionDate | DateTime | - | Transaction date and time |
| CreatedAt | DateTime | - | Record creation timestamp |
| CreatedBy | Varchar | 50 | Username who created the transaction |
| IsDeleted | Bit | 1 | Soft delete flag (0=Active, 1=Deleted) |
| DeletedAt | DateTime | - | Deletion timestamp |
| DeletedBy | Varchar | 50 | Username who deleted the record |

---

## 14. SyncConfigurations Table

| Field Name | Data Type | Length | Description |
|------------|-----------|--------|-------------|
| SyncConfigurationId (PK) | Int - AI | 9 | Configuration's unique identifier (Primary Key, Auto-Increment) |
| EntityName | Varchar | 100 | Entity name to sync (e.g., Product, SalesOrder) - Unique |
| IsEnabled | Bit | 1 | Sync enabled flag (1=Enabled, 0=Disabled) |
| Priority | Int | 9 | Sync priority order (lower number = higher priority) |
| Direction | Varchar | 20 | Sync direction (Bidirectional, LocalToCloud, CloudToLocal) |
| LastSyncedAt | DateTime | - | Last successful sync timestamp |
| CreatedAt | DateTime | - | Record creation timestamp |
| UpdatedAt | DateTime | - | Last update timestamp |

---

## 15. SyncLogs Table

| Field Name | Data Type | Length | Description |
|------------|-----------|--------|-------------|
| SyncLogId (PK) | Int - AI | 9 | Log entry's unique identifier (Primary Key, Auto-Increment) |
| SessionId | UniqueIdentifier | 36 | Sync session GUID |
| EntityName | Varchar | 100 | Entity name being synced |
| Operation | Varchar | 20 | Operation type (Push, Pull, Conflict) |
| Status | Varchar | 20 | Operation status (Success, Failed, Warning) |
| ErrorMessage | Text | - | Error message if failed |
| RecordsProcessed | Int | 9 | Number of records processed |
| StartedAt | DateTime | - | Operation start timestamp |
| CompletedAt | DateTime | - | Operation completion timestamp |

---

## 16. SyncMetadata Table

| Field Name | Data Type | Length | Description |
|------------|-----------|--------|-------------|
| SyncMetadataId (PK) | Int - AI | 9 | Metadata's unique identifier (Primary Key, Auto-Increment) |
| EntityName | Varchar | 100 | Entity name |
| EntityId | Varchar | 50 | Entity's primary key value |
| LastSyncedAt | DateTime | - | Last sync timestamp for this entity |
| DataHash | Varchar | 64 | Hash of entity data for change detection |
| SyncDirection | Varchar | 20 | Last sync direction |
| Status | Varchar | 20 | Sync status (Completed, Pending, Failed) |
| CreatedAt | DateTime | - | Record creation timestamp |
| UpdatedAt | DateTime | - | Last update timestamp |

---

## Database Summary

**Total Tables:** 16 active tables

**Table Categories:**
- **Core System:** 5 tables (User, Role, Permission, RolePermission, Department)
- **Inventory:** 8 tables (Category, Product, SalesOrder, SalesOrderLine, PurchaseOrder, PurchaseOrderLine, InventoryTransaction, RevenueTransaction)
- **Sync System:** 3 tables (SyncConfiguration, SyncLog, SyncMetadata)

**Key Conventions:**
- **PK** = Primary Key
- **FK** = Foreign Key
- **AI** = Auto-Increment
- **Decimal(18,2)** = 18 digits total, 2 decimal places
- **Bit** = Boolean (0 or 1)
- **Soft Delete** = Records marked as deleted but not physically removed

**Naming Conventions:**
- Primary Keys: TableName + "Id" (e.g., UserId, ProductId)
- Foreign Keys: Referenced table name + "Id" (e.g., CategoryId, RoleId)
- Timestamps: CreatedAt, UpdatedAt, DeletedAt
- Audit Fields: CreatedBy, UpdatedBy, DeletedBy (stores username)

---

**Generated:** 2025-10-01  
**System:** ModularSys ERP  
**Version:** 1.0
