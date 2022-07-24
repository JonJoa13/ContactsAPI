using System.ComponentModel.DataAnnotations;

namespace ContactsAPI.Models
{
    public class UpdateContactRequest
    {
        [Required]
        public string Firstname { get; set; }
        [Required] 
        public string Lastname { get; set; }
        [Required] 
        public string Fullname { get; set; }
        [Required] 
        public string Address { get; set; }
        [Required] 
        public string Email { get; set; }
        [Required] 
        public string MobilePhoneNumber { get; set; }
    }
}
