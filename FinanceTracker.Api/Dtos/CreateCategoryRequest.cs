using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Api.Dtos;

public class CreateCategoryRequest
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Type { get; set; } = string.Empty; 
}