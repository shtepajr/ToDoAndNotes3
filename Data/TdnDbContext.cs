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
            builder.Entity<Project>()
                .HasQueryFilter(p => !p.IsDeleted);
            builder.Entity<Project>()
                .HasMany(p => p.Tasks)
                .WithOne(pt => pt.Project)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<Project>()
                .HasMany(p => p.Notes)
                .WithOne(pn => pn.Project)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Models.Task>()
                .HasQueryFilter(t => !t.IsDeleted);

            builder.Entity<Models.Task>()
                .HasMany(t => t.TaskLabels)
                .WithOne(tl => tl.Task)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<Models.Label>()
                .HasMany(l => l.TaskLabels)
                .WithOne(tl => tl.Label)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Note>()
                .HasQueryFilter(n => !n.IsDeleted);

            builder.Entity<Note>()
                .HasMany(t => t.NoteLabels)
                .WithOne(nl => nl.Note)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<Models.Label>()
                .HasMany(l => l.NoteLabels)
                .WithOne(nl => nl.Label)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(builder);
        }
        public DbSet<Project> Projects { get; set; } = default!;
        public DbSet<Models.Task> Tasks { get; set; } = default!;
        public DbSet<Note> Notes { get; set; } = default!;
        public DbSet<Models.Label> Labels { get; set; } = default!;
    }
}
