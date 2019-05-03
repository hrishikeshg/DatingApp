using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.DTOs
{
    public class UserRegister
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [StringLength(8, MinimumLength = 4,ErrorMessage = "You must specify password between 4 to 8 character.")]
        public string Password { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }
    }
}