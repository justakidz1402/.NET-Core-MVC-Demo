using BookStore.Models.Domain;

namespace BookStore.Models
{
    public class UpdateCategoryViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool Status { get; set; }
        public ICollection<Book> Books { get; set; }
    }
}
