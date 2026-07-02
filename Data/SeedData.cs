using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Data
{
    /// <summary>
    /// Handles seeding of initial data into the database.
    /// Seeds 5 sample books on first run when the Books table is empty.
    /// Seeded books have AddedBy set to "system" to distinguish them
    /// from books added via the API by actual clients.
    /// </summary>
    public static class SeedData
    {
        /// <summary>
        /// Seeds the database with sample book records if no books exist.
        /// Called once during application startup from Program.cs.
        /// If the Books table already has records, seeding is skipped.
        /// </summary>
        /// <param name="serviceProvider">
        /// The service provider used to resolve AppDbContext from DI container.
        /// </param>
        public static void Initialize(IServiceProvider serviceProvider)
        {
            // Resolve AppDbContext from the DI container
            using var context = serviceProvider.GetRequiredService<AppDbContext>();

            // Check if books already exist in the database
            // If yes, skip seeding to avoid duplicate records
            if (context.Books.Any())
            {
                // Data already seeded, nothing to do
                return;
            }

            var books = new List<Book>
            {
                new Book
                {
                    Title         = "Clean Code",
                    Author        = "Robert C. Martin",
                    ISBN          = "978-0132350884",
                    PublishedYear = 2008,
                    AddedBy       = "system",
                    CreatedAt     = DateTime.Now
                },
                new Book
                {
                    Title         = "The Pragmatic Programmer",
                    Author        = "Andrew Hunt",
                    ISBN          = "978-0201616224",
                    PublishedYear = 1999,
                    AddedBy       = "system",
                    CreatedAt     = DateTime.Now
                },
                new Book
                {
                    Title         = "Design Patterns",
                    Author        = "Gang of Four",
                    ISBN          = "978-0201633610",
                    PublishedYear = 1994,
                    AddedBy       = "system",
                    CreatedAt     = DateTime.Now
                },
                new Book
                {
                    Title         = "Head First Design Patterns",
                    Author        = "Eric Freeman",
                    ISBN          = "978-0596007126",
                    PublishedYear = 2004,
                    AddedBy       = "system",
                    CreatedAt     = DateTime.Now
                },
                new Book
                {
                    Title         = "Refactoring",
                    Author        = "Martin Fowler",
                    ISBN          = "978-0201485677",
                    PublishedYear = 1999,
                    AddedBy       = "system",
                    CreatedAt     = DateTime.Now
                }
            };

            // Add all seed books to the context
            context.Books.AddRange(books);

            // Persist seed data to the database
            context.SaveChanges();
        }
    }
}