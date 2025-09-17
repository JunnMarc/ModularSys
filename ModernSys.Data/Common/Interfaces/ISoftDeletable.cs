namespace ModularSys.Data.Common.Interfaces;

public interface ISoftDeletable
{
    // Soft delete flags
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
    string? DeletedBy { get; set; }

    // Audit fields
    DateTime? CreatedAt { get; set; }
    string? CreatedBy { get; set; }
    DateTime? UpdatedAt { get; set; }
    string? UpdatedBy { get; set; }
}
