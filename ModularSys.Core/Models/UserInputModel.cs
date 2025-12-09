using System.ComponentModel.DataAnnotations;

namespace ModularSys.Core.Models;

public class UserInputModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone number format")]
    [StringLength(20, ErrorMessage = "Contact number cannot exceed 20 characters")]
    public string? ContactNumber { get; set; }

    [Required(ErrorMessage = "Role is required")]
    public int RoleId { get; set; }

    [Required(ErrorMessage = "Department is required")]
    public int DepartmentId { get; set; }

    // Password fields (only for create/password change)
    // For new users: can use temporary password (< 12 chars) or permanent password (>= 12 chars)
    [StringLength(100, ErrorMessage = "Password cannot be longer than 100 characters")]
    public string Password { get; set; } = string.Empty;

    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;

    // For edit mode
    public bool IsEditMode => Id > 0;
    public bool RequirePassword => !IsEditMode;
    
    // Indicates if this is a temporary password (< 12 chars) that requires change on first login
    public bool IsTemporaryPassword => !string.IsNullOrEmpty(Password) && Password.Length < 12;
}

public class UserSearchModel
{
    public string? SearchTerm { get; set; }
    public int? RoleId { get; set; }
    public int? DepartmentId { get; set; }
    public bool IncludeDeleted { get; set; } = false;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
