using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookStore.Models.Domain;
using Microsoft.Data.SqlClient;

namespace BookStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly BookStoreDbContext _context;

        public BooksController(BookStoreDbContext context)
        {
            _context = context;
        }

        // GET: api/Books
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks()
        {
          if (_context.Books == null)
          {
              return NotFound();
          }
            return await _context.Books.ToListAsync();
        }

        [HttpGet("Search")]
        public async Task<ActionResult<IEnumerable<Book>>> SearchBooks(
            Guid? id = null,
            string? title = null,
            bool? status = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                var idParam = new SqlParameter("@Id", (object?)id?.ToString() ?? DBNull.Value);
                var titleParam = new SqlParameter("@Title", (object?)title ?? DBNull.Value);
                var statusParam = new SqlParameter("@Status", (object?)status ?? DBNull.Value);
                var pageNumberParam = new SqlParameter("@PageNumber", pageNumber);
                var pageSizeParam = new SqlParameter("@PageSize", pageSize);

                var books = await _context.Set<Book>()
                    .FromSqlRaw("EXEC SearchProduct @Id, @Title, @Status, @PageNumber, @PageSize",
                        idParam, titleParam, statusParam, pageNumberParam, pageSizeParam)
                    .ToListAsync();

                if (books == null || !books.Any())
                {
                    return NotFound("No books found matching the criteria.");
                }

                return Ok(books);
            }
            catch (Exception ex)
            {
                // Log the exception here
                return StatusCode(500, "An error occurred while searching for books. Please try again later.");
            }
        }

        [HttpGet("Category")]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooksByCategoryId(Guid? id = null)
        {
            try
            {
                var idParam = new SqlParameter("@CategoryId", (object?)id?.ToString() ?? DBNull.Value);

                var books = await _context.Set<Book>()
                    .FromSqlRaw("EXEC GetProductByCategoryId @CategoryId", idParam)
                    .ToListAsync();

                if (books == null || !books.Any())
                {
                    return NotFound("No books found matching the criteria.");
                }

                return Ok(books);
            }
            catch (Exception ex)
            {
                // Log the exception here
                return StatusCode(500, "An error occurred while searching for books. Please try again later.");
            }
        }

        // GET: api/Books/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Book>> GetBook(Guid id)
        {
          if (_context.Books == null)
          {
              return NotFound();
          }
            var book = await _context.Books.FindAsync(id);

            if (book == null)
            {
                return NotFound();
            }

            return book;
        }

        // PUT: api/Books/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBook(Guid id, Book book)
        {
            if (id != book.Id)
            {
                return BadRequest();
            }

            _context.Entry(book).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Books
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Book>> PostBook(Book book)
        {
          if (_context.Books == null)
          {
              return Problem("Entity set 'BookStoreDbContext.Books'  is null.");
          }
            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBook", new { id = book.Id }, book);
        }

        // DELETE: api/Books/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(Guid id)
        {
            if (_context.Books == null)
            {
                return NotFound();
            }
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BookExists(Guid id)
        {
            return (_context.Books?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
