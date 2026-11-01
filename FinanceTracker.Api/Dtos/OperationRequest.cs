using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Api.Dtos;

public class OperationRequest
{
    [Required]
    public Guid AccountId { get; set; }

    [Required]
    public Guid CategoryId { get; set; }

    [Required]
    public string Type { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
    public decimal Amount { get; set; }

    [Required]
    public DateOnly Date { get; set; }

    [MaxLength(500)]
    public string? Comment { get; set; }
}