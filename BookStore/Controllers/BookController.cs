using BookStore.Areas.Identity.Data;
using BookStore.Models;
using BookStore.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Controllers
{
    public class BookController : Controller
    {
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly BookStoreDbContext bookStoreDbContext;
        public BookController(IWebHostEnvironment webHostEnvironment, BookStoreDbContext bookStoreDbContext)
        {
            this.webHostEnvironment = webHostEnvironment;
            this.bookStoreDbContext = bookStoreDbContext;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Index(string searchString, int? pageNumber)
        {
            int pageSize = 5;
            var books = from b in bookStoreDbContext.Books
                        join c in bookStoreDbContext.Categories on b.CategoryId equals c.Id
                        select new BookViewModel
                        {
                            Id = b.Id,
                            Title = b.Title,
                            ImagePath = b.ImagePath,
                            Author = b.Author,
                            Publisher = b.Publisher,
                            Price = b.Price,
                            Description = b.Description,
                            Created = b.Created,
                            CategoryId = b.CategoryId,
                            CategoryName = c.Name,
                            Status = b.Status,
                            Quantity = b.Quantity,
                        };


            if (!string.IsNullOrEmpty(searchString))
            {
                books = books.Where(x => x.Title.Contains(searchString));
            }

            return View(await PaginatedList<BookViewModel>.CreateAsync(books.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        [HttpGet]
        public async Task<IActionResult> View(string searchString, Guid? categoryId, long? minPrice, long? maxPrice, int? pageNumber)
        {
            int pageSize = 12;
            var books = from b in bookStoreDbContext.Books
                        join c in bookStoreDbContext.Categories on b.CategoryId equals c.Id
                        where b.Status == true
                        select new BookViewModel
                        {
                            Id = b.Id,
                            Title = b.Title,
                            ImagePath = b.ImagePath,
                            Author = b.Author,
                            Publisher = b.Publisher,
                            Price = b.Price,
                            Description = b.Description,
                            Created = b.Created,
                            CategoryId = b.CategoryId,
                            CategoryName = c.Name,
                            Status = b.Status,
                            Quantity = b.Quantity,
                        };

            if (!string.IsNullOrEmpty(searchString))
            {
                books = books.Where(x => x.Title.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(categoryId.ToString()))
            {
                books = books.Where(x => x.CategoryId == categoryId);
            }

            if (minPrice.HasValue)
            {
                books = books.Where(x => x.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                books = books.Where(x => x.Price <= maxPrice.Value);
            }

            ViewBag.Categories = await bookStoreDbContext.Categories
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync();

            ViewBag.CurrentSearchString = searchString;
            ViewBag.CurrentCategoryId = categoryId;
            ViewBag.CurrentMinPrice = minPrice;
            ViewBag.CurrentMaxPrice = maxPrice;

            return View(await PaginatedList<BookViewModel>.CreateAsync(books.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        [HttpGet]
        public async Task<IActionResult> Detail(Guid id)
        {
            var book = await bookStoreDbContext.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            var category = await bookStoreDbContext.Categories.FindAsync(book.CategoryId);
            if (category == null)
            {
                return NotFound();
            }

            var viewModel = new BookViewModel
            {
                Id = book.Id,
                Title = book.Title,
                ImagePath = book.ImagePath,
                Author = book.Author,
                Publisher = book.Publisher,
                Price = book.Price,
                Description = book.Description,
                Created = book.Created,
                CategoryId = book.CategoryId,
                CategoryName = category.Name,
                Quantity = book.Quantity,
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Add()
        {
            var viewModel = new AddBookViewModel
            {
                Categories = bookStoreDbContext.Categories
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    })
                    .ToList()
            };
            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Add(AddBookViewModel addBookModel)
        {
            if (ModelState.IsValid)
            {
                string? imageUpload = null;
                if (addBookModel.ImageFile != null)
                {
                    string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "images");
                    imageUpload = Guid.NewGuid().ToString() + "_" + addBookModel.ImageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, imageUpload);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await addBookModel.ImageFile.CopyToAsync(fileStream);
                    }
                }

                var category = await bookStoreDbContext.Categories.FindAsync(addBookModel.CategoryId);

                var book = new Book()
                {
                    Id = Guid.NewGuid(),
                    Title = addBookModel.Title,
                    ImagePath = imageUpload,
                    Author = addBookModel.Author,
                    Publisher = addBookModel.Publisher,
                    Price = addBookModel.Price,
                    Quantity = addBookModel.Quantity,
                    Description = addBookModel.Description,
                    Created = addBookModel.Created,
                    CategoryId = addBookModel.CategoryId,
                    Status = category.Status,
                };

                try
                {
                    await bookStoreDbContext.Books.AddAsync(book);
                    await bookStoreDbContext.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            addBookModel.Categories = bookStoreDbContext.Categories
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToList();
            return View(addBookModel);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Update(Guid id)
        {
            var book = await bookStoreDbContext.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            var viewModel = new UpdateBookViewModel
            {
                Id = book.Id,
                Title = book.Title,
                ImagePath = book.ImagePath,
                Author = book.Author,
                Publisher = book.Publisher,
                Price = book.Price,
                Description = book.Description,
                Quantity = book.Quantity,
                Created = book.Created,
                CategoryId = book.CategoryId,
                Categories = bookStoreDbContext.Categories
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    })
                    .ToList()
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Update(UpdateBookViewModel updateBookModel)
        {
            if (ModelState.IsValid)
            {
                var book = await bookStoreDbContext.Books.FindAsync(updateBookModel.Id);
                if (book == null)
                {
                    return NotFound();
                }

                if (updateBookModel.ImageFile != null)
                {
                    if (!string.IsNullOrEmpty(book.ImagePath))
                    {
                        var oldImagePath = Path.Combine(webHostEnvironment.WebRootPath, "images", book.ImagePath);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "images");
                    string imageUpload = Guid.NewGuid().ToString() + "_" + updateBookModel.ImageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, imageUpload);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await updateBookModel.ImageFile.CopyToAsync(fileStream);
                    }
                    book.ImagePath = imageUpload;
                }

                book.Title = updateBookModel.Title;
                book.Author = updateBookModel.Author;
                book.Publisher = updateBookModel.Publisher;
                book.Price = updateBookModel.Price;
                book.Description = updateBookModel.Description;
                book.Created = updateBookModel.Created;
                book.CategoryId = updateBookModel.CategoryId;
                book.Quantity = updateBookModel.Quantity;

                try
                {
                    bookStoreDbContext.Update(book);
                    await bookStoreDbContext.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(updateBookModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }

            updateBookModel.Categories = bookStoreDbContext.Categories
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToList();
            return View(updateBookModel);
        }

        private bool BookExists(Guid id)
        {
            return bookStoreDbContext.Books.Any(e => e.Id == id);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> ChangeStatus(UpdateBookViewModel model)
        {
            var book = await bookStoreDbContext.Books.FindAsync(model.Id);
            if (book != null)
            {
                var category = await bookStoreDbContext.Categories.FindAsync(book.CategoryId);
                if (category != null && category.Status)
                {
                    try
                    {
                        book.Status = !book.Status;
                        await bookStoreDbContext.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        TempData["ErrorMessage"] = ex.Message;
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "You can't change when category's status is false!";
                }
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(UpdateBookViewModel model)
        {
            var book = await bookStoreDbContext.Books.FindAsync(model.Id);
            if (book != null)
            {
                if (!string.IsNullOrEmpty(book.ImagePath))
                {
                    string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "images");
                    string filePath = Path.Combine(uploadsFolder, book.ImagePath);
                    if (System.IO.File.Exists(filePath))
                    {
                        try
                        {
                            System.IO.File.Delete(filePath);
                        }
                        catch (IOException ex)
                        {

                        }
                    }
                }

                bookStoreDbContext.Books.Remove(book);
                await bookStoreDbContext.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
