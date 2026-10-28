using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Api.Dtos;

public class UpdateCategoryRequest
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
}
