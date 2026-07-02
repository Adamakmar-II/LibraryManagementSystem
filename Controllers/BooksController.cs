using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LibraryManagementSystem.Controllers
{
    /// <summary>
    /// Handles CRUD operations for the Books resource.
    /// All endpoints require a valid JWT Bearer token in the Authorization header.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("{version}/api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<BooksController> _logger;

        /// <summary>
        /// Initializes BooksController with required dependencies.
        /// </summary>
        /// <param name="context">Database context for data access.</param>
        /// <param name="logger">Logger for recording book operation events.</param>
        public BooksController(AppDbContext context, ILogger<BooksController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves books from the database.
        /// 
        /// GET: v1/api/Books        => Get all books
        /// GET: v1/api/Books?id=1   => Get book with Id = 1
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> GetBooks([FromQuery] int? id)
        {
            // ── Get By Id ──────────────────────────────────────────────
            if (id.HasValue)
            {
                // Look up book by primary key
                var book = await _context.Books.FindAsync(id.Value);

                // Book not found, log warning and return 404
                if (book == null)
                {
                    _logger.LogWarning(
                        "GetById | Book not found | Id: {Id}", id.Value);

                    return NotFound(new
                    {
                        success = false,
                        message = $"Book with Id {id.Value} not found."
                    });
                }

                // Log the retrieved book details when getting by Id
                _logger.LogInformation(
                    "GetById | Id: {Id} | Title: {Title} | Author: {Author} | " +
                    "ISBN: {ISBN} | PublishedYear: {Year} | AddedBy: {AddedBy}",
                    book.Id, book.Title, book.Author,
                    book.ISBN, book.PublishedYear, book.AddedBy);

                // Map entity to DTO to format CreatedAt for API response
                var responseData = BookResponseDto.FromEntity(book);

                return Ok(new
                {
                    success = true,
                    data = responseData
                });
            }

            // ── Get All ────────────────────────────────────────────────
            // No id specified, return all books
            var books = await _context.Books.ToListAsync();

            // Map each entity to DTO to format CreatedAt for API response
            var responseList = books.Select(BookResponseDto.FromEntity).ToList();

            return Ok(new
            {
                success = true,
                totalRecords = responseList.Count,
                data = responseList
            });
        }

        /// <summary>
        /// Creates a new book record in the database.
        /// 
        /// POST: v1/api/Books
        /// 
        /// Request body: Title, Author, ISBN, PublishedYear only.
        /// Do NOT include AddedBy or CreatedAt in the request body.
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreateBook([FromBody] Book book)
        {
            // Validate the incoming request body against model rules
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Extract ClientId from JWT token claims
            // ClaimTypes.Name was embedded during token generation in TokenService
            var clientId = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";

            // Set AddedBy from the authenticated ClientId in the token
            book.AddedBy = clientId;

            // Set CreatedAt to current datetime
            // Stored in DB with full precision e.g. 2026-07-01 22:54:36.310
            book.CreatedAt = DateTime.Now;

            // Add the new book to the EF context and save to DB
            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            // Map entity to DTO to format CreatedAt for API response
            var responseData = BookResponseDto.FromEntity(book);

            // Log the created book
            _logger.LogInformation(
                "Create | Id: {Id} | Title: {Title} | Author: {Author} | " +
                "ISBN: {ISBN} | PublishedYear: {Year} | AddedBy: {AddedBy}",
                book.Id, book.Title, book.Author,
                book.ISBN, book.PublishedYear, book.AddedBy);

            return Ok(new
            {
                success = true,
                message = "Book created successfully.",
                data = responseData
            });
        }

        /// <summary>
        /// Updates an existing book record identified by Id.
        /// 
        /// PUT: v1/api/Books?id=1
        /// Request body: Title, Author, ISBN, PublishedYear.
        /// </summary>
        [HttpPut("update")]
        public async Task<IActionResult> UpdateBook([FromQuery] int id, [FromBody] Book updatedBook)
        {
            // Find the existing book record by primary key
            var book = await _context.Books.FindAsync(id);

            // Return 404 if book not found
            if (book == null)
            {
                // Log warning when book to update is not found
                _logger.LogWarning("Update | Book not found | Id: {Id}", id);

                return NotFound(new
                {
                    success = false,
                    message = $"Book with Id {id} not found."
                });
            }

            // Update only the allowed fields from the request body
            // AddedBy and CreatedAt are intentionally not updated
            book.Title = updatedBook.Title;
            book.Author = updatedBook.Author;
            book.ISBN = updatedBook.ISBN;
            book.PublishedYear = updatedBook.PublishedYear;

            // Persist the updated book to the database
            await _context.SaveChangesAsync();

            // Map entity to DTO to format CreatedAt for API response
            var responseData = BookResponseDto.FromEntity(book);

            // Log the updated book
            _logger.LogInformation(
                "Update | Id: {Id} | Title: {Title} | Author: {Author} | " +
                "ISBN: {ISBN} | PublishedYear: {Year}",
                book.Id, book.Title, book.Author,
                book.ISBN, book.PublishedYear);

            return Ok(new
            {
                success = true,
                message = "Book updated successfully.",
                data = responseData
            });
        }

        /// <summary>
        /// Deletes a book record identified by Id.
        /// 
        /// DELETE: v1/api/Books?id=1
        /// </summary>
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteBook([FromQuery] int id)
        {
            // Find the existing book record by primary key
            var book = await _context.Books.FindAsync(id);

            // Return 404 if book not found
            if (book == null)
            {
                // Log warning when book to delete is not found
                _logger.LogWarning("Delete | Book not found | Id: {Id}", id);

                return NotFound(new
                {
                    success = false,
                    message = $"Book with Id {id} not found."
                });
            }

            // Remove the book from EF context and save to DB
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            // Log the deleted book
            _logger.LogInformation(
                "Delete | Id: {Id} | Title: {Title} | Author: {Author}",
                book.Id, book.Title, book.Author);

            return Ok(new
            {
                success = true,
                message = $"Book with Id {id} deleted successfully."
            });
        }
    }
}