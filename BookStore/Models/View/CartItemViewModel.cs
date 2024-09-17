using BookStore.Models.Domain;

namespace BookStore.Models
{
    public class CartItemViewModel
    {
        public Guid BookId { get; set; }
        public int Quantity { get; set; }
        public Book Book { get; set; }

        public long TotalPrice => Quantity * Book.Price;
    }
}
