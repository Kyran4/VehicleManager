using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehicleManager.Core.Domain;
using VehicleManager.Core.Rules;

namespace VehicleManager.Infrastructure.Persistence.Configurations;

internal sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles", t =>
            t.HasCheckConstraint("CK_Vehicles_WeightKg_Positive", "[WeightKg] > 0"));

        builder.Property(v => v.OwnerName).HasMaxLength(VehicleValidator.MaxOwnerNameLength).IsRequired();
        builder.Property(v => v.WeightKg).HasPrecision(10, 2);

        // No category column on purpose: category is derived from WeightKg at read time.
        builder.HasOne(v => v.Manufacturer)
            .WithMany()
            .HasForeignKey(v => v.ManufacturerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(v => v.ManufacturerId);
    }
}
