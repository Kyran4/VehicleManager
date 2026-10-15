using VehicleManager.Core.Abstractions;
using VehicleManager.Core.Common;
using VehicleManager.Core.Domain;
using VehicleManager.Core.Rules;

namespace VehicleManager.Core.Services;

/// <summary>
/// Category administration. Every change is applied to an in-memory copy of the whole configuration,
/// validated as a set, and only then persisted - inside one transaction.
/// </summary>
public sealed class CategoryService(ICategoryRepository repository)
{
    public async Task<IReadOnlyList<CategoryBand>> GetBandsAsync(CancellationToken ct = default) =>
        CategoryBands.Build(await repository.GetAllAsync(ct));

    public async Task<CategoryBand?> GetBandAsync(int id, CancellationToken ct = default) =>
        (await GetBandsAsync(ct)).FirstOrDefault(b => b.Category.Id == id);

    public Task<Result<int>> CreateAsync(CategoryInput input, CancellationToken ct = default) =>
        repository.ExecuteInTransactionAsync(async () =>
        {
            var definitionErrors = CategoryConfigurationValidator.ValidateDefinition(input.Name, input.IconKey, input.MinWeightKg);
            if (definitionErrors.Count > 0)
                return Result<int>.From(Result.Invalid(definitionErrors));

            var category = ToEntity(0, input);
            var proposed = (await repository.GetAllAsync(ct)).Append(category).ToList();

            var errors = CategoryConfigurationValidator.Validate(proposed);
            if (errors.Count > 0)
                return Result<int>.From(Result.Invalid(errors));

            await repository.AddAsync(category, ct);
            return Result<int>.Ok(category.Id);
        }, ct);

    public Task<Result> UpdateAsync(int id, CategoryInput input, CancellationToken ct = default) =>
        repository.ExecuteInTransactionAsync(async () =>
        {
            var existing = await repository.GetAllAsync(ct);
            if (existing.All(c => c.Id != id))
                return Result.NotFound("That category no longer exists.");

            var definitionErrors = CategoryConfigurationValidator.ValidateDefinition(input.Name, input.IconKey, input.MinWeightKg);
            if (definitionErrors.Count > 0)
                return Result.Invalid(definitionErrors);

            var updated = ToEntity(id, input);
            var proposed = existing.Select(c => c.Id == id ? updated : c).ToList();

            var errors = CategoryConfigurationValidator.Validate(proposed);
            if (errors.Count > 0)
                return Result.Invalid(errors);

            await repository.UpdateAsync(updated, ct);
            return Result.Ok();
        }, ct);

    /// <summary>
    /// Deleting a category lets its lower neighbour absorb its range. If the lowest category is deleted,
    /// the next one is extended down to 0 kg. The last remaining category cannot be deleted.
    /// Vehicles are unaffected because their category is calculated, not stored.
    /// </summary>
    public Task<Result> DeleteAsync(int id, CancellationToken ct = default) =>
        repository.ExecuteInTransactionAsync(async () =>
        {
            var all = await repository.GetAllAsync(ct);
            var target = all.FirstOrDefault(c => c.Id == id);
            if (target is null)
                return Result.NotFound("That category no longer exists.");

            var remaining = all.Where(c => c.Id != id).ToList();
            if (remaining.Count == 0)
                return Result.Invalid(ValidationError.General,
                    "The last remaining category cannot be deleted - every vehicle weight must map to a category.");

            VehicleCategory? promoted = null;
            if (target.MinWeightKg == all.Min(c => c.MinWeightKg))
            {
                var next = remaining.MinBy(c => c.MinWeightKg)!;
                promoted = new VehicleCategory { Id = next.Id, Name = next.Name, IconKey = next.IconKey, MinWeightKg = 0m };
                remaining[remaining.FindIndex(c => c.Id == next.Id)] = promoted;
            }

            var errors = CategoryConfigurationValidator.Validate(remaining);
            if (errors.Count > 0)
                return Result.Invalid(errors);

            // Delete first so the promoted category's new 0 kg start never collides with the unique index.
            await repository.DeleteAsync(target, ct);
            if (promoted is not null)
                await repository.UpdateAsync(promoted, ct);

            return Result.Ok();
        }, ct);

    // Callers have already passed ValidateDefinition, so the nullable inputs are populated.
    private static VehicleCategory ToEntity(int id, CategoryInput input) => new()
    {
        Id = id,
        Name = input.Name!.Trim(),
        IconKey = input.IconKey!,
        MinWeightKg = input.MinWeightKg!.Value,
    };
}
