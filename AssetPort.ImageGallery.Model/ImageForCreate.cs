using System.ComponentModel.DataAnnotations;

namespace AssetPort.ImageGallery.Model
{
    public class ImageForCreate
    {
        [Required]
        [MaxLength(150)]
        public string Title { get; set; }

        [Required]
        public byte[] Bytes { get; set; }
    }
}
