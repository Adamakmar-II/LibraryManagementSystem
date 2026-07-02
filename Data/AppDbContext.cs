using LibraryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace LibraryManagementSystem.Data
{
    /// <summary>
    /// Database context for the Library Management System.
    /// Manages the connection to the LocalDb database and entity mappings.
    /// </summary>
    public class AppDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of AppDbContext with the given options.
        /// </summary>
        /// <param name="options">Database context options injected via DI.</param>
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        /// <summary>
        /// Represents the Books table in the database.
        /// </summary>
        public DbSet<Book> Books { get; set; }
    }
}