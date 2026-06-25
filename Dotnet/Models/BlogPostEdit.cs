using System.ComponentModel.DataAnnotations;

namespace Dotnet.Models
{
    public class BlogPostEdit
    
    {
        public int PostId { get; set; }

        public string Tittle { get; set; } = null!;

        public string? PostDescription { get; set; }

        public string Content { get; set; } = null!;

        public DateTime PublishedDate { get; set; }

        public short? AuthorId { get; set; }

        public string? EncId { get; set; }

        [DataType(DataType.Upload)]

        public IFormFile? BlogFile { get; set; }

        public string? UploadUserName { get; set; }
    
        public string? UserProfile { get; set; }
    }

}

