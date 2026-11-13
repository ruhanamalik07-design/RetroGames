
using System.ComponentModel.DataAnnotations;

namespace RetroGames.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public decimal Price { get; set; }

        public int CategoryId { get; set; }

        public Category? Category { get; set; }
    }
}
