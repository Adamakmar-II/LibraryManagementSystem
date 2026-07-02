namespace LibraryManagementSystem.Models
{
    public class BookResponseDto
    {
        /// <summary>
        /// Primary key of the book.
        /// </summary>
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Author { get; set; } = string.Empty;

        public string ISBN { get; set; } = string.Empty;

        public int PublishedYear { get; set; }

        public string AddedBy { get; set; } = string.Empty;

        public string CreatedAt { get; set; } = string.Empty;

        /// <summary>
        /// Maps a Book entity (from DB) to BookResponseDto (for API response).
        /// Formats CreatedAt from full DateTime to a readable string.
        /// </summary>
        /// <param name="book">The Book entity retrieved from database.</param>
        /// <returns>A BookResponseDto with formatted CreatedAt.</returns>
        public static BookResponseDto FromEntity(Book book)
        {
            return new BookResponseDto
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                ISBN = book.ISBN,
                PublishedYear = book.PublishedYear,
                AddedBy = book.AddedBy,
                // Format DateTime to friendly display string
                // e.g. 2024-12-15 2:30 PM
                CreatedAt = book.CreatedAt.ToString("yyyy-MM-dd h:mm tt")
            };
        }
    }
}