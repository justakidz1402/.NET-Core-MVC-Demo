using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace BookStore.Models
{
    public class AddBookViewModel
    {
        public string Title { get; set; }
        [Display(Name = "Image")]
        public IFormFile ImageFile { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
        public long Price { get; set; }
        public long Quantity { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }
        public Guid CategoryId { get; set; }
        public List<SelectListItem>? Categories { get; set; }
    }
}
