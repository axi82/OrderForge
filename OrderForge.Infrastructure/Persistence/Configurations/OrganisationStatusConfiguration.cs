using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderForge.Domain.Organisations;

namespace OrderForge.Infrastructure.Persistence.Configurations;

public class OrganisationStatusConfiguration : IEntityTypeConfiguration<OrganisationStatus>
{
    public void Configure(EntityTypeBuilder<OrganisationStatus> builder)
    {
        builder.ToTable("organisation_statuses");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(s => s.Code).HasColumnName("code").IsRequired().HasMaxLength(32);

        builder.HasIndex(s => s.Code).IsUnique();

        builder.HasData(
            new OrganisationStatus { Id = OrganisationStatus.ActiveId, Code = "Active" },
            new OrganisationStatus { Id = OrganisationStatus.InactiveId, Code = "Inactive" },
            new OrganisationStatus { Id = OrganisationStatus.UnknownId, Code = "Unknown" });
    }
}
