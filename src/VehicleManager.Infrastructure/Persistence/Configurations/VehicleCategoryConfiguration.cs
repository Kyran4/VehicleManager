using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehicleManager.Core.Domain;
using VehicleManager.Core.Rules;

namespace VehicleManager.Infrastructure.Persistence.Configurations;

internal sealed class VehicleCategoryConfiguration : IEntityTypeConfiguration<VehicleCategory>
{
    public void Configure(EntityTypeBuilder<VehicleCategory> builder)
    {
        builder.ToTable("VehicleCategories", t =>
            t.HasCheckConstraint("CK_VehicleCategories_MinWeightKg_NonNegative", "[MinWeightKg] >= 0"));

        builder.Property(c => c.Name).HasMaxLength(CategoryConfigurationValidator.MaxNameLength).IsRequired();
        builder.Property(c => c.IconKey).HasMaxLength(30).IsRequired();
        builder.Property(c => c.MinWeightKg).HasPrecision(10, 2);

        // Backstops for the service-level rules: no duplicate names, and no two categories can start at
        // the same weight (which would be an overlap).
        builder.HasIndex(c => c.Name).IsUnique();
        builder.HasIndex(c => c.MinWeightKg).IsUnique();

        builder.HasData(
            new VehicleCategory { Id = 1, Name = "Light", IconKey = "feather", MinWeightKg = 0m },
            new VehicleCategory { Id = 2, Name = "Medium", IconKey = "car", MinWeightKg = 500m },
            new VehicleCategory { Id = 3, Name = "Heavy", IconKey = "truck", MinWeightKg = 2500m });
    }
}
