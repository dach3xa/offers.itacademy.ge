using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using offers.Domain.Models;
using System.Reflection.Emit;

namespace offers.Persistance.Configurations
{
    public class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.ToTable("Accounts");

            builder.HasKey(x => x.Id);

            builder.Ignore(u => u.PhoneNumberConfirmed);
            builder.Ignore(u => u.TwoFactorEnabled);
            builder.Ignore(u => u.AccessFailedCount);
            builder.Ignore(u => u.LockoutEnd);
            builder.Ignore(u => u.LockoutEnabled);
            builder.Ignore(u => u.NormalizedEmail);
            builder.Ignore(u => u.NormalizedUserName);
            builder.Ignore(u => u.ConcurrencyStamp);
            builder.Ignore(u => u.SecurityStamp);


            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(u => u.PhoneNumber)
                .HasMaxLength(50)
                .IsUnicode(false);

            builder.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(512)
                .IsUnicode(false);

            builder.Property(u => u.Role)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsUnicode(false);
        }
    }
}
