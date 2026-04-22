using GameDataViewer.Core.Models;

namespace GameDataViewer.Core.Contracts;

public interface IItemRepository
{
    Task<PageResult<Item>> GetPagedAsync(PageRequest request, CancellationToken ct = default);
    Task<Item?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetGradesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken ct = default);
    Task<int> GetTotalCountAsync(CancellationToken ct = default);
}

public interface ISkillRepository
{
    Task<PageResult<Skill>> GetPagedAsync(PageRequest request, CancellationToken ct = default);
    Task<Skill?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetClassesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetTypesAsync(CancellationToken ct = default);
    Task<int> GetTotalCountAsync(CancellationToken ct = default);
}

public interface IMonsterRepository
{
    Task<PageResult<Monster>> GetPagedAsync(PageRequest request, CancellationToken ct = default);
    Task<Monster?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetGradesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetLocationsAsync(CancellationToken ct = default);
    Task<int> GetTotalCountAsync(CancellationToken ct = default);
}
