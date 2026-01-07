using Microsoft.EntityFrameworkCore;
using PhotosApi.Helpers;
using PhotosApi.Models;

namespace PhotosApi.Infrastructure.Data;

public class PhotosDbContext : DbContext
{
    public PhotosDbContext(DbContextOptions<PhotosDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<Photo> Photos => Set<Photo>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<PhotoCategories> PhotosCategories => Set<PhotoCategories>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");
            entity.HasKey(e => e.CategoryId);
            entity.Property(e => e.CategoryId).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Ignore(e => e.CategoryType);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<Photo>(entity =>
        {
            entity.ToTable("photos");
            entity.HasKey(e => e.PhotoId);
            entity.Property(e => e.PhotoId).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.S3FilePath).HasColumnName("s3_file_path");
            entity.Property(e => e.UploadedAt).HasColumnName("uploaded_at").HasDefaultValueSql("current_timestamp");
            entity.Property(e => e.ShootYear).HasColumnName("shoot_year");
            entity.HasIndex(e => e.S3FilePath).IsUnique();
        });

        modelBuilder.Entity<PhotoCategories>(entity =>
        {
            entity.ToTable("photo_categories");
            entity.HasKey(e => new {e.PhotoId, e.CategoryId});
            entity.Property(e => e.PhotoId).HasColumnName("photo_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            
            entity.HasOne(e => e.Photo)
                .WithMany(p => p.PhotosToCategory)
                .HasForeignKey(e => e.PhotoId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Category)
                .WithMany(c => c.PhotosToCategory)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });


    }
}