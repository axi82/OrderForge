using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderForge.Domain.Organisations;

namespace OrderForge.Infrastructure.Persistence.Configurations;

public class OrganisationConfiguration : IEntityTypeConfiguration<Organisation>
{
    public void Configure(EntityTypeBuilder<Organisation> builder)
    {
        builder.ToTable("organisations");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).HasColumnName("id").UseIdentityByDefaultColumn();

        builder.Property(o => o.Name).HasColumnName("name").IsRequired().HasMaxLength(200);

        builder.Property(o => o.TradingAs).HasMaxLength(200).HasColumnName("trading_as");

        builder.Property(o => o.CompanyNumber).HasMaxLength(50).HasColumnName("company_number");

        builder.Property(o => o.VatNumber).HasMaxLength(20).HasColumnName("vat_number");

        builder.Property(o => o.AccountNumber).HasMaxLength(20).HasColumnName("account_number");

        builder
            .Property(o => o.OrganisationStatusId)
            .HasColumnName("organisation_status_id")
            .IsRequired()
            .HasDefaultValue(OrganisationStatus.ActiveId);

        builder
            .HasOne(o => o.OrganisationStatus)
            .WithMany()
            .HasForeignKey(o => o.OrganisationStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Property(o => o.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()");

        builder
            .Property(o => o.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("now()");
    }
}
