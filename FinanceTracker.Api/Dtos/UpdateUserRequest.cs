using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Api.Dtos;

public class UpdateUserRequest
{
    [Required]
    [MinLength(1)]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}