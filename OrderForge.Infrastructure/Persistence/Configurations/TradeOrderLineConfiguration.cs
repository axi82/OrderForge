using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderForge.Domain.Orders;

namespace OrderForge.Infrastructure.Persistence.Configurations;

public class TradeOrderLineConfiguration : IEntityTypeConfiguration<TradeOrderLine>
{
    public void Configure(EntityTypeBuilder<TradeOrderLine> builder)
    {
        builder.ToTable("trade_order_lines");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).HasColumnName("id").UseIdentityByDefaultColumn();

        builder.Property(l => l.TradeOrderId).HasColumnName("trade_order_id").IsRequired();

        builder.Property(l => l.SortOrder).HasColumnName("sort_order").IsRequired();

        builder
            .Property(l => l.Description)
            .HasColumnName("description")
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(l => l.Quantity).HasColumnName("quantity").HasPrecision(18, 2).IsRequired();

        builder.Property(l => l.LineTotal).HasColumnName("line_total").HasPrecision(18, 2).IsRequired();

        builder
            .HasOne(l => l.TradeOrder)
            .WithMany(o => o.Lines)
            .HasForeignKey(l => l.TradeOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(l => new { l.TradeOrderId, l.SortOrder });
    }
}
