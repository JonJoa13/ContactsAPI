namespace ContactsAPI.Models
{
    public class ContactSkill
    {
        public Guid ContactId { get; set; }
        public Guid SkillId { get; set; }
        public Contact Contact { get; set; }
        public Skill Skill { get; set; }
    }
}
