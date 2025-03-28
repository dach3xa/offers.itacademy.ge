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
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasKey(x => x.Id);  

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(255) 
                .IsUnicode(false);

            builder.Property(x => x.Description)
                .HasMaxLength(1000) 
                .IsUnicode(false);  

            builder.HasMany(c => c.Offer)
                .WithOne(o => o.Category)
                .HasForeignKey(o => o.CategoryId);
        }
    }
}
