using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderForge.Domain.Orders;

namespace OrderForge.Infrastructure.Persistence.Configurations;

public class TradeOrderConfiguration : IEntityTypeConfiguration<TradeOrder>
{
    public void Configure(EntityTypeBuilder<TradeOrder> builder)
    {
        builder.ToTable("trade_orders");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).HasColumnName("id").UseIdentityByDefaultColumn();

        builder.Property(o => o.OrganisationId).HasColumnName("organisation_id").IsRequired();

        builder
            .Property(o => o.OrderNumber)
            .HasColumnName("order_number")
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(o => o.PlacedAt).HasColumnName("placed_at").IsRequired();

        builder.Property(o => o.Status).HasColumnName("status").IsRequired().HasMaxLength(50);

        builder.Property(o => o.Total).HasColumnName("total").HasPrecision(18, 2).IsRequired();

        builder
            .HasOne(o => o.Organisation)
            .WithMany()
            .HasForeignKey(o => o.OrganisationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(o => new { o.OrganisationId, o.PlacedAt });
    }
}
