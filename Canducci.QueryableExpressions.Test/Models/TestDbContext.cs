using Microsoft.EntityFrameworkCore;

namespace Canducci.QueryableExpressions.Test.Models
{
    public class TestDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200).HasColumnType("TEXT COLLATE NOCASE");
                entity.Property(e => e.Gender).HasMaxLength(10);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdateAt).IsRequired(false);
                entity.Property(e => e.Price).IsRequired(true).HasDefaultValue(0);
                entity.Property(e => e.Active).IsRequired(true).HasDefaultValue(false);
            });
        }
    }
}