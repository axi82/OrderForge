using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderForge.Domain.Products;

namespace OrderForge.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").UseIdentityByDefaultColumn();

        builder.Property(p => p.Sku).HasColumnName("sku").IsRequired().HasMaxLength(100);
        builder.Property(p => p.ProductCode).HasColumnName("product_code").IsRequired().HasMaxLength(100);
        builder.Property(p => p.Name).HasColumnName("name").IsRequired().HasMaxLength(300);
        builder.Property(p => p.ShortDescription).HasColumnName("short_description").HasMaxLength(500);
        builder.Property(p => p.Description).HasColumnName("description");
        builder.Property(p => p.Brand).HasColumnName("brand").HasMaxLength(200);

        builder.Property(p => p.CostPrice).HasColumnName("cost_price").HasPrecision(18, 2);
        builder.Property(p => p.BasePrice).HasColumnName("base_price").HasPrecision(18, 2);

        builder
            .Property(p => p.CommodityCodeDescription)
            .HasColumnName("commodity_code_description")
            .HasMaxLength(100);

        builder.Property(p => p.SupplierAccountCode).HasColumnName("supplier_account_code").HasMaxLength(50);
        builder.Property(p => p.PartNumber).HasColumnName("part_number").HasMaxLength(100);

        builder.Property(p => p.QuantityInStock).HasColumnName("quantity_in_stock").HasPrecision(18, 2);
        builder.Property(p => p.QuantityAllocated).HasColumnName("quantity_allocated").HasPrecision(18, 2);
        builder.Property(p => p.QuantityOnOrder).HasColumnName("quantity_on_order").HasPrecision(18, 2);
        builder.Property(p => p.FreeStock).HasColumnName("free_stock").HasPrecision(18, 2);

        builder.Property(p => p.Barcode).HasColumnName("barcode").HasMaxLength(64);

        builder.Property(p => p.IsActive).HasColumnName("is_active").IsRequired();

        builder
            .Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()");

        builder
            .Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("now()");

        builder.Property(p => p.CreatedBy).HasColumnName("created_by").IsRequired().HasMaxLength(128);

        builder.HasIndex(p => p.Sku).IsUnique();
        builder.HasIndex(p => p.ProductCode).IsUnique();
    }
}
