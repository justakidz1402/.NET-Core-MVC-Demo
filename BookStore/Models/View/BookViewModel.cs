﻿namespace BookStore.Models
{
    public class BookViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string ImagePath { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }
        public long Quantity { get; set; }
        public bool Status { get; set; }
    }
}
