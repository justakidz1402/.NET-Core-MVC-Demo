using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models.Domain
{
    public class Book
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string? ImagePath { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
        public long Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime Created { get; set; }
        public bool Status { get; set; } = true;
        public long Quantity { get; set; }
        public Guid CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
