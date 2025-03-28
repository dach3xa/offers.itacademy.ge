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
    public class OfferConfiguration : IEntityTypeConfiguration<Offer>
    {
        public void Configure(EntityTypeBuilder<Offer> builder)
        {
            builder.HasKey(x => x.Id); 

            builder.Property(x => x.Name)
                .IsRequired() 
                .HasMaxLength(255) 
                .IsUnicode(false); 

            builder.Property(x => x.Description)
                .HasMaxLength(1000)
                .IsUnicode(false); 

            builder.Property(x => x.Count)
                .IsRequired(); 

            builder.Property(x => x.Price)
                .IsRequired(); 

            builder.Property(x => x.CreatedAt)
                .IsRequired(); 

            builder.Property(x => x.ArchiveAt)
                .IsRequired();

            builder.Property(x => x.IsArchived)
                .IsRequired();

            builder.HasOne(x => x.Category)
                .WithMany(x => x.Offer)
                .HasForeignKey(x => x.CategoryId);

            builder.HasOne(x => x.Account)
                .WithMany(x => x.Offers)
                .HasForeignKey(x => x.AccountId);

            builder.HasMany(x => x.Transactions)
                .WithOne(x => x.Offer);
        }
    }
}