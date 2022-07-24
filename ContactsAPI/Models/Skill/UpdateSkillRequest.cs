using System.ComponentModel.DataAnnotations;

namespace ContactsAPI.Models
{
    public class UpdateSkillRequest
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Level { get; set; }
    }
}
