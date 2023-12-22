using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ToDoAndNotes3.Models;

namespace ToDoAndNotes3.Data
{
    public class TdnDbContext : IdentityDbContext<User>
    {
        public TdnDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }
        public DbSet<Project> Projects { get; set; } = default!;
        public DbSet<Models.Task> Tasks { get; set; } = default!;
        public DbSet<Note> Notes { get; set; } = default!;
        public DbSet<Label> Labels { get; set; } = default!;
    }
}
