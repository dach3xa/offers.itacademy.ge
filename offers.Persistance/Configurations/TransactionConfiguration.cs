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
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.HasKey(x => x.Id); 

            builder.Property(x => x.Count)
                .IsRequired();

            builder.Property(x => x.Paid)
                .IsRequired()
                .HasColumnType("decimal(18,2)"); 

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime");

            builder.HasOne(x => x.User)
                .WithMany(x => x.Transactions) 
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Offer)
                .WithMany(o => o.Transactions) 
                .HasForeignKey(x => x.OfferId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
