using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderForge.Domain.Products;

namespace OrderForge.Infrastructure.Persistence.Configurations;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("product_images");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id").UseIdentityByDefaultColumn();

        builder.Property(i => i.ProductId).HasColumnName("product_id").IsRequired();

        builder
            .Property(i => i.StoragePath)
            .HasColumnName("storage_path")
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(i => i.SortOrder).HasColumnName("sort_order").IsRequired();

        builder.Property(i => i.IsMain).HasColumnName("is_main").IsRequired();

        builder
            .Property(i => i.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()");

        builder
            .HasIndex(i => i.ProductId)
            .IsUnique()
            .HasFilter("is_main = TRUE");

        builder
            .HasOne(i => i.Product)
            .WithMany(p => p.Images)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
