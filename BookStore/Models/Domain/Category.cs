namespace BookStore.Models.Domain
{
    public class Category
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool Status { get; set; } = true;
        public ICollection<Book> Books { get; set; }
    }
}
