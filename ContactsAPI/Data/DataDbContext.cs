using ContactsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ContactsAPI.Data
{
    public class ContactsApiDbContext : DbContext
    {
        public ContactsApiDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<ContactSkill> ContactSkills { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Contact>()
                .Property(c => c.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Skill>()
                .Property(s => s.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<User>()
                .HasMany(c => c.Contacts)
                .WithOne(u => u.User)
                .HasForeignKey(c => c.UserId);

            modelBuilder.Entity<ContactSkill>()
                .HasKey(cs => new { cs.ContactId, cs.SkillId });
            modelBuilder.Entity<ContactSkill>()
                .HasOne(c => c.Contact)
                .WithMany(cs => cs.ContactSkills)
                .HasForeignKey(c => c.ContactId);
            modelBuilder.Entity<ContactSkill>()
                .HasOne(s => s.Skill)
                .WithMany(cs => cs.ContactSkills)
                .HasForeignKey(s => s.SkillId);

        }
    }
}
