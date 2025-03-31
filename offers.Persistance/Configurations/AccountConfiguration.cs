using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using offers.Domain.Models;

namespace offers.Persistance.Configurations
{
    public class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(u => u.Phone)
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
