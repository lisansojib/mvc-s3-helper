using System.Data.Entity;

namespace MVC_S3_Helper.Models
{
    public class ImageUploadDBContext : DbContext
    {
        public ImageUploadDBContext() : base("DefaultConnection") { }
        public DbSet<Image> Images { get; set; }
    }
}