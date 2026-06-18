using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dotnet.Models
{
    public class UserListEdit
    {
        public short UserId { get; set; }

        public string FullName { get; set; } = null!;

        public string EmailAddress { get; set; } = null!;

        public string UsePhoto { get; set; } = null!;

        public string UserRole { get; set; } = null!;

        public string UserPassword { get; set; } = null!;

        public string CurrentAddress { get; set; } = null!;

        public string EncID { get; set; } = null!;

        [DataType(DataType.Upload)]
        public IFormFile? UserFile { get; set; } = null!;

        public string EmailToken { get; set; } = null!;

        public Boolean? IsEmailConfirmed { get; set; }

    }
}
