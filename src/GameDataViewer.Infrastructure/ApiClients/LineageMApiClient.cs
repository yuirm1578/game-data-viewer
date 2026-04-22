using System.Net.Http.Json;
using GameDataViewer.Core.Contracts;
using GameDataViewer.Core.Models;
using Microsoft.Extensions.Logging;

namespace GameDataViewer.Infrastructure.ApiClients;

/// <summary>
/// 리니지M 스타일 게임 API HTTP 클라이언트 구현체.
/// <para>
/// 실제 API 연동 방법:
///   1. appsettings 또는 환경 변수에서 BaseAddress, ApiKey를 읽도록 생성자를 수정합니다.
///   2. <see cref="FetchItemsAsync"/> 등의 메서드의 엔드포인트 경로를 실제 경로로 교체합니다.
///   3. App.xaml.cs의 DI 등록 주석을 해제합니다.
/// </para>
/// </summary>
public sealed class LineageMApiClient : IGameApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<LineageMApiClient> _logger;

    // TODO: 실제 API BaseAddress 와 ApiKey 를 환경 변수 또는 설정 파일에서 주입받도록 수정하세요.
    private const string DefaultBaseAddress = "https://api.lineagem.example.com/v1/";

    public LineageMApiClient(HttpClient http, ILogger<LineageMApiClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Item>> FetchItemsAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching items from API: {Url}", _http.BaseAddress);
        try
        {
            // TODO: 실제 엔드포인트 경로로 교체하세요.
            var result = await _http.GetFromJsonAsync<List<Item>>("items", ct);
            return result ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch items from API");
            return [];
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Skill>> FetchSkillsAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching skills from API: {Url}", _http.BaseAddress);
        try
        {
            var result = await _http.GetFromJsonAsync<List<Skill>>("skills", ct);
            return result ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch skills from API");
            return [];
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Monster>> FetchMonstersAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching monsters from API: {Url}", _http.BaseAddress);
        try
        {
            var result = await _http.GetFromJsonAsync<List<Monster>>("monsters", ct);
            return result ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch monsters from API");
            return [];
        }
    }

    /// <summary>
    /// DI에서 HttpClient 를 구성할 때 사용할 기본 주소.
    /// App.xaml.cs에서 AddHttpClient 호출 시 사용합니다.
    /// </summary>
    public static string BaseAddress => DefaultBaseAddress;
}
