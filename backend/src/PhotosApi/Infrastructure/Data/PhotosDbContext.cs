using Microsoft.EntityFrameworkCore;
using PhotosApi.Models;

namespace PhotosApi.Infrastructure.Data;

public class PhotosDbContext : DbContext
{
    public PhotosDbContext(DbContextOptions<PhotosDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<Photo> Photos => Set<Photo>();
    public DbSet<Album> Albums => Set<Album>();
    public DbSet<Category> Categories => Set<Category>();
    
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
            
            entity.HasMany(c => c.Albums)
                .WithOne(a => a.Category)
                .HasForeignKey(a => a.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Album>(entity =>
        {
            entity.ToTable("albums");
            entity.HasKey(e => e.AlbumId);
            entity.Property(e => e.AlbumId).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("current_timestamp");
            entity.Property(e => e.ShootYear).HasColumnName("shoot_year");
            
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.HasIndex(e => e.CategoryId);
            
            entity.Property(e => e.VisibilityStatus).HasColumnName("viability_status").IsRequired()
                .HasConversion<string>();
            
            entity.HasMany(a => a.Photos)
                .WithOne(a => a.Album)
                .HasForeignKey(a => a.AlbumId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<Photo>(entity =>
        {
            entity.ToTable("photos");
            entity.HasKey(e => e.PhotoId);
            entity.Property(e => e.PhotoId).HasColumnName("id").IsRequired()
                .HasDefaultValueSql("uuid_generate_v4()");
            
            entity.Property(e => e.AlbumId).HasColumnName("album_id").IsRequired();
            
            entity.Property(e => e.UploadedAt).HasColumnName("uploaded_at")
                .HasDefaultValueSql("current_timestamp");
            
            entity.HasIndex(e => e.AlbumId);
        });

    }
}