namespace LibraryManagementSystem.Models
{
    public class Book
    {
        /// <summary>
        /// Primary key, auto incremented by database.
        /// </summary>
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Author { get; set; } = string.Empty;

        public string ISBN { get; set; } = string.Empty;

        public int PublishedYear { get; set; }

        /// <summary>
        /// ClientId of the user who added the book.
        /// Auto populated from JWT token claims on create.
        /// Seeded records use "system" as the value.
        /// Not required in request body.
        /// </summary>
        public string AddedBy { get; set; } = string.Empty;

        /// <summary>
        /// e.g. 2026-07-01 22:54:36.310 (SQL Server datetime2).
        /// Auto populated on create, not required in request body.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}