namespace BookStore.Models.Domain
{
    public class CartItem
    {
        public Guid Id { get; set; }
        public Guid BookId { get; set; }
        public int Quantity { get; set; }
        public string? UserId { get; set; }
    }
}
