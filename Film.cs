using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("film")]
public class Film
{
    [Key]
    public int film_id { get; set; }

    [Required]
    public string title { get; set; } = string.Empty;

    public string? description { get; set; }

    public string? release_year { get; set; }

    public byte? language_id { get; set; }

    public byte? original_language_id { get; set; }

    public byte? rental_duration { get; set; }

    [Column(TypeName = "decimal(4,2)")]
    public decimal? rental_rate { get; set; }

    public short? length { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? replacement_cost { get; set; }

    public string? rating { get; set; }

    public string? special_features { get; set; }

    public DateTime? last_update { get; set; }

    public byte[]? embeddings { get; set; }
}