using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Outbox
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; init; } = Guid.NewGuid();

    [Required]
    [MaxLength(64)]
    public string Category { get; set; } = default!;

    [Required]
    [MaxLength(4096)]
    public string Type { get; set; } = default!;

    [Required]
    [MaxLength(4096)]
    public string Content { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
}
