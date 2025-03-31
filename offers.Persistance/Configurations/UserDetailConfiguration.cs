using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using offers.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Persistance.Configurations
{
    public class UserDetailConfiguration : IEntityTypeConfiguration<UserDetail>
    {
        public void Configure(EntityTypeBuilder<UserDetail> builder)
        {
            builder.HasKey(x => x.AccountId);  

            builder.Property(x => x.FirstName)
                .IsRequired()
                .HasMaxLength(100)  
                .IsUnicode(false);  

            builder.Property(x => x.LastName)
                .IsRequired()
                .HasMaxLength(100)  
                .IsUnicode(false);  

            builder.Property(x => x.Balance)
                .IsRequired();  

            builder.HasOne(x => x.Account)  
                .WithOne(x => x.UserDetail)  
                .HasForeignKey<UserDetail>(x => x.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
