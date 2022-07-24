using System.ComponentModel.DataAnnotations;

namespace ContactsAPI.Models
{
    public class AddUserRequest
    {
        [Required]
        public string Username { get; set; }
        [Required] 
        public string Password { get; set; }
        [Required]
        public string Role { get; set; }
    }
}
