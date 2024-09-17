using BookStore.Areas.Identity.Data;
using BookStore.Models;
using BookStore.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Controllers
{
    public class CategoryController : Controller
    {
        private readonly BookStoreDbContext bookStoreDbContext;
        public CategoryController(BookStoreDbContext bookStoreDbContext)
        {
            this.bookStoreDbContext = bookStoreDbContext;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Index(string searchString, int? pageNumber)
        {
            int pageSize = 5;
            var categories = from c in bookStoreDbContext.Categories select c;
            if (!string.IsNullOrEmpty(searchString))
            {
                categories = categories.Where(s => s.Name.Contains(searchString));
            }
            return View(await PaginatedList<Category>.CreateAsync(categories.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Add(AddCategoryViewModel addCategoryModel)
        {
            var category = new Category()
            {
                Id = Guid.NewGuid(),
                Name = addCategoryModel.Name,
                Description = addCategoryModel.Description,
                Status = true,
            };

            await bookStoreDbContext.Categories.AddAsync(category);
            await bookStoreDbContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Update(Guid id)
        {
            var category = await bookStoreDbContext.Categories.FirstOrDefaultAsync(c => c.Id == id);

            if (category != null) {
                var viewModel = new UpdateCategoryViewModel()
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    Status = category.Status,
                };
                return await Task.Run(() => View("Update", viewModel));
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Update(UpdateCategoryViewModel model)
        {
            var category = await bookStoreDbContext.Categories.FindAsync(model.Id);
            if (category != null) { 
                category.Name = model.Name;
                category.Description = model.Description;

                await bookStoreDbContext.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> ChangeStatus(UpdateCategoryViewModel model)
        {
            var category = await bookStoreDbContext.Categories
                .Include(c => c.Books)
                .FirstOrDefaultAsync(c => c.Id == model.Id);

            if (category != null)
            {
                category.Status = !category.Status;
                foreach (var product in category.Books)
                {
                    product.Status = category.Status;
                }

                await bookStoreDbContext.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}
