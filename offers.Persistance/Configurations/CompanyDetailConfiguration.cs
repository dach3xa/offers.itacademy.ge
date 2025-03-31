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
    public class CompanyDetailConfiguration : IEntityTypeConfiguration<CompanyDetail>
    {
        public void Configure(EntityTypeBuilder<CompanyDetail> builder)
        {
            builder.HasKey(x => x.AccountId); 

            builder.Property(x => x.CompanyName)
                .IsRequired() 
                .HasMaxLength(255)  
                .IsUnicode(false); 

            builder.Property(x => x.IsActive)
                .IsRequired(); 

            builder.HasOne(x => x.Account)  
                .WithOne(x => x.CompanyDetail)  
                .HasForeignKey<CompanyDetail>(x => x.AccountId)
                .OnDelete(DeleteBehavior.Restrict);  
        }
    }

}
