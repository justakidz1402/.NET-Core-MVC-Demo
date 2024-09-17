using BookStore.Areas.Identity.Data;
using BookStore.Models;
using BookStore.Models.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BookStore.Controllers
{
    public class CartController : Controller
    {
        private readonly BookStoreDbContext bookStoreDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartController(BookStoreDbContext dbContext, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            bookStoreDbContext = dbContext;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> Index()
        {
            var cartItems = await GetCartItemsAsync();
            var bookIds = cartItems.Select(ci => ci.BookId).ToList();
            var books = await bookStoreDbContext.Books.Where(b => bookIds.Contains(b.Id)).ToListAsync();

            var viewModel = cartItems.Select(ci => new CartItemViewModel
            {

                BookId = ci.BookId,
                Quantity = ci.Quantity,
                Book = books.First(b => b.Id == ci.BookId)
            }).ToList();

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(Guid bookId, int quantity)
        {
            await AddToCartAsync(bookId, quantity);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(Guid bookId)
        {
            await RemoveFromCartAsync(bookId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            await ClearCartAsync();
            return RedirectToAction("Index");
        }

        private async Task<List<CartItem>> GetCartItemsAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                return await bookStoreDbContext.CartItems.Where(ci => ci.UserId == user.Id).ToListAsync();
            }
            else
            {
                var cartJson = Request.Cookies["Cart"];
                return string.IsNullOrEmpty(cartJson) ? new List<CartItem>() : JsonSerializer.Deserialize<List<CartItem>>(cartJson);
            }
        }

        private async Task AddToCartAsync(Guid bookId, int quantity)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var cartItem = await bookStoreDbContext.CartItems.FirstOrDefaultAsync(ci => ci.BookId == bookId && ci.UserId == user.Id);
                if (cartItem == null)
                {
                    cartItem = new CartItem { BookId = bookId, Quantity = quantity, UserId = user.Id };
                    bookStoreDbContext.CartItems.Add(cartItem);
                }
                else
                {
                    cartItem.Quantity += quantity;
                }
                await bookStoreDbContext.SaveChangesAsync();
            }
            else
            {
                var cartItems = await GetCartItemsAsync();
                var cartItem = cartItems.FirstOrDefault(ci => ci.BookId == bookId);
                if (cartItem == null)
                {
                    cartItems.Add(new CartItem { BookId = bookId, Quantity = quantity });
                }
                else
                {
                    cartItem.Quantity += quantity;
                }
                var cartJson = JsonSerializer.Serialize(cartItems);
                Response.Cookies.Append("Cart", cartJson, new CookieOptions { Expires = DateTime.Now.AddDays(7) });
            }
        }

        private async Task RemoveFromCartAsync(Guid bookId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var cartItem = await bookStoreDbContext.CartItems.FirstOrDefaultAsync(ci => ci.BookId == bookId && ci.UserId == user.Id);
                if (cartItem != null)
                {
                    bookStoreDbContext.CartItems.Remove(cartItem);
                    await bookStoreDbContext.SaveChangesAsync();
                }
            }
            else
            {
                var cartItems = await GetCartItemsAsync();
                cartItems.RemoveAll(ci => ci.BookId == bookId);
                var cartJson = JsonSerializer.Serialize(cartItems);
                Response.Cookies.Append("Cart", cartJson, new CookieOptions { Expires = DateTime.Now.AddDays(7) });
            }
        }

        private async Task ClearCartAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var cartItems = await bookStoreDbContext.CartItems.Where(ci => ci.UserId == user.Id).ToListAsync();
                bookStoreDbContext.CartItems.RemoveRange(cartItems);
                await bookStoreDbContext.SaveChangesAsync();
            }
            else
            {
                Response.Cookies.Delete("Cart");
            }
        }

        [HttpGet]
        public async Task<int> GetCartItemCount()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
                return await bookStoreDbContext.CartItems.Where(ci => ci.UserId == user.Id).CountAsync();
                //return await bookStoreDbContext.CartItems.Where(ci => ci.UserId == user.Id).SumAsync(ci => ci.Quantity);
            }
            else
            {
                var cartCookie = Request.Cookies["Cart"];
                if (!string.IsNullOrEmpty(cartCookie))
                {
                    var cartItems = JsonSerializer.Deserialize<List<CartItem>>(cartCookie);
                    //return cartItems.Sum(ci => ci.Quantity);
                    return cartItems.Count();
                }
            }

            return 0;
        }
    }
}
