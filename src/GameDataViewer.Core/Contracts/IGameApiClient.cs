using GameDataViewer.Core.Models;

namespace GameDataViewer.Core.Contracts;

/// <summary>
/// 외부 게임 API와 통신하는 클라이언트 인터페이스.
/// 실제 리니지M API가 공개될 경우 이 인터페이스를 구현하여 DataSeeder 대신 사용합니다.
/// </summary>
public interface IGameApiClient
{
    /// <summary>아이템 목록을 API에서 조회합니다.</summary>
    Task<IReadOnlyList<Item>> FetchItemsAsync(CancellationToken ct = default);

    /// <summary>스킬 목록을 API에서 조회합니다.</summary>
    Task<IReadOnlyList<Skill>> FetchSkillsAsync(CancellationToken ct = default);

    /// <summary>몬스터 목록을 API에서 조회합니다.</summary>
    Task<IReadOnlyList<Monster>> FetchMonstersAsync(CancellationToken ct = default);
}
