namespace Turnero.Web.Models;

public sealed class CreatePublicTurnRequest
{
    [Required, StringLength(150)]
    public string? Name { get; set; }

    [Required, Range(100_000, 9_999_999_999)]
    public long? Dni { get; set; }

    [Required]
    public Guid MedicId { get; set; }

    [Required, RegularExpression(@"^\d{4}-\d{2}-\d{2}$")]
    public string? Date { get; set; }

    [Required]
    public Guid TimeId { get; set; }

    [StringLength(100)]
    public string? SocialWork { get; set; }

    [StringLength(500)]
    public string? Reason { get; set; }

    [Required, Range(100_000_000, 9_999_999_999)]
    public long? Phone { get; set; }
}
