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
    public DbSet<Album> Albums => Set<Album>();
    public DbSet<Category> Categories => Set<Category>();
    
    public DbSet<ShowcasePage> ShowcasePages => Set<ShowcasePage>();
    public DbSet<ShowcaseSectionType> ShowcaseSectionTypes => Set<ShowcaseSectionType>();
    public DbSet<ShowcaseSection> ShowcaseSections => Set<ShowcaseSection>();
    public DbSet<ShowcaseItem> ShowcaseItems => Set<ShowcaseItem>();
    
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
            
            entity.Property(e => e.VisibilityStatus).HasColumnName("visibility_status").IsRequired()
                .HasConversion<string>();
            
            entity.Property(e => e.CoverPhotoId).HasColumnName("cover_photo_id");
            
            entity.HasOne(a => a.CoverPhoto)
                .WithMany()
                .HasForeignKey(a => a.CoverPhotoId)
                .OnDelete(DeleteBehavior.Cascade);
            
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

        modelBuilder.Entity<ShowcasePage>(entity =>
        {
            entity.ToTable("showcase_pages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.PageKey).HasColumnName("page_key").IsRequired();
            entity.HasIndex(e => e.PageKey).IsUnique();
        });
        
        modelBuilder.Entity<ShowcaseSectionType>(entity =>
        {
            entity.ToTable("showcase_section_types");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.Removable).HasColumnName("removable").HasDefaultValue(true);
            entity.HasIndex(e => e.Name).IsUnique();
        });
        
        modelBuilder.Entity<ShowcaseSection>(entity =>
        {
            entity.ToTable("showcase_sections");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PageId).HasColumnName("page_id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.SectionKey).HasColumnName("section_key").IsRequired();
            entity.Property(e => e.ShowcaseSectionTypeId).HasColumnName("showcase_section_type_id");
            entity.Property(e => e.DisplayOrder).HasColumnName("display_order");

            entity.HasIndex(e => new { e.PageId, e.SectionKey }).IsUnique();

            entity.HasOne(e => e.Page)
                .WithMany(p => p.Sections)
                .HasForeignKey(e => e.PageId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.SectionType)
                .WithMany()
                .HasForeignKey(e => e.ShowcaseSectionTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ShowcaseItem>(entity =>
        {
            entity.ToTable("showcase_items");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SectionId).HasColumnName("section_id");

            entity.Property(e => e.ShowcaseItemType)
                .HasColumnName("item_type")
                .HasConversion(
                    v => v.ToString().ToLower(),
                    v => Enum.Parse<ShowcaseItemType>(v, true)
                )
                .IsRequired();

            entity.Property(e => e.PhotoId).HasColumnName("photo_id");
            entity.Property(e => e.AlbumId).HasColumnName("album_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("current_timestamp");
            entity.Property(e => e.DisplayOrder).HasColumnName("display_order");
            
            entity.HasOne(e => e.Section)
                .WithMany(s => s.Items)
                .HasForeignKey(e => e.SectionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Album)
                .WithMany()
                .HasForeignKey(e => e.AlbumId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Photo)
                .WithMany()
                .HasForeignKey(e => e.PhotoId)
                .OnDelete(DeleteBehavior.Cascade);
        });


    }
}