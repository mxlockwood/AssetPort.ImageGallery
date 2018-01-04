using AssetPort.ImageGallery.API.Entities;
using Microsoft.EntityFrameworkCore;


namespace AssetPort.ImageGallery.API.Entities
{
    public class GalleryContext : DbContext
    {
        public GalleryContext(DbContextOptions<GalleryContext> options) : base(options)
        {
        }

        public DbSet<Image> Images { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Image>().ToTable("Image");
        }
    }
}
