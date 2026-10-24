using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Api.Dtos;

public class UpdateAccountRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string Currency { get; set; } = string.Empty;

    public decimal StartAmount { get; set; }
}
