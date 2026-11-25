
using Microsoft.EntityFrameworkCore;
using DocumentManagement.Models;

namespace DocumentManagement.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Document> Documents => Set<Document>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Document>(entity =>
            {
                entity.HasKey(d => d.DocumentId);

                entity.Property(d => d.FileName).HasMaxLength(255).IsRequired();

                entity.Property(d => d.StoredFileName).HasMaxLength(255).IsRequired();

                entity.Property(d => d.ContentType).HasMaxLength(150).IsRequired();

                entity.Property(d => d.FileSize).IsRequired();

                entity.Property(d => d.UploadDate).HasColumnType("datetime2").IsRequired();

                entity.HasIndex(d => d.UploadDate);
            });
        }
    }
}