using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection.Emit;
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
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Project>(entityType => entityType.HasQueryFilter((Project e) => !e.IsDeleted));
            builder.Entity<Models.Task>(entityType => entityType.HasQueryFilter((Models.Task e) => !e.IsDeleted));
            builder.Entity<Note>(entityType => entityType.HasQueryFilter((Note e) => !e.IsDeleted));

            base.OnModelCreating(builder);
        }
        public DbSet<Project> Projects { get; set; } = default!;
        public DbSet<Models.Task> Tasks { get; set; } = default!;
        public DbSet<Note> Notes { get; set; } = default!;
        public DbSet<Models.Label> Labels { get; set; } = default!;
    }
}
