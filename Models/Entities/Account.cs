using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArqanumServer.Models.Entities
{
    public class Account
    {
        public string Id { get; set; }

        public string Username { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Bio { get; set; }

        public byte[] SignaturePublicKey { get; set; }
    }

    public class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Id)
                   .ValueGeneratedNever();

            builder.Property(a => a.Username)
                   .IsRequired()
                   .HasMaxLength(32);

            builder.HasIndex(a => a.Username)
                   .IsUnique();

            builder.Property(a => a.FirstName)
                   .HasMaxLength(32);

            builder.Property(a => a.LastName)
                   .HasMaxLength(32);

            builder.Property(a => a.Bio)
                   .HasMaxLength(150);

            builder.Property(a => a.SignaturePublicKey)
                   .IsRequired();
        }
    }
}
