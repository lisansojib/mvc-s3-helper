using System.ComponentModel.DataAnnotations;

namespace MVC_S3_Helper.Models
{
    public class Image
    {
        public int ID { get; set; }

        [Required]
        [Display(Name = "Image Title")]
        public string ImageTitle { get; set; }

        [Display(Name = "File Name")]
        public string OriginalFileName { get; set; }

        [Display(Name = "Image File")]
        public string ImagePath { get; set; }

        public string ImagePathS3 { get; set; }
    }
}