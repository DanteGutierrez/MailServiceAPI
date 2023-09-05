using Microsoft.EntityFrameworkCore;

namespace MailServiceAPI.Data
{
    public class MailDBContext : DbContext
    {
        public MailDBContext()
        {

        }
        public MailDBContext(DbContextOptions<MailDBContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Reciever)
                .WithMany(u => u.RecievedMessages)
                .HasForeignKey(m => m.RecieverId)
                .HasPrincipalKey(u => u.Id);
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .HasPrincipalKey(u => u.Id);
        }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Message> Messages { get; set; }
    }
}
