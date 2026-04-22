using System.Text.Json;
using Dapper;
using GameDataViewer.Core.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace GameDataViewer.Infrastructure.Data;

/// <summary>JSON 시드 파일 → SQLite DB 적재 (INSERT OR REPLACE Upsert 전략)</summary>
public class DataSeeder
{
    private readonly DatabaseContext _db;
    private readonly ILogger<DataSeeder> _logger;
    private readonly string _dataDirectory;

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public DataSeeder(DatabaseContext db, ILogger<DataSeeder> logger, string? dataDirectory = null)
    {
        _db = db;
        _logger = logger;
        _dataDirectory = dataDirectory ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
    }

    /// <summary>
    /// 시드를 실행한다.
    /// forceReseed=true 이면 기존 데이터를 모두 삭제 후 재적재한다.
    /// </summary>
    public async Task SeedAsync(bool forceReseed = false)
    {
        using var conn = _db.CreateConnection();

        if (forceReseed)
        {
            _logger.LogWarning("Force reseed requested — truncating all tables.");
            await conn.ExecuteAsync("DELETE FROM Items; DELETE FROM Skills; DELETE FROM Monsters;");
        }
        else
        {
            var itemCount = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Items");
            if (itemCount > 0)
            {
                _logger.LogInformation("Seed data already present ({Count} items). Skipping.", itemCount);
                return;
            }
        }

        _logger.LogInformation("Seeding data from {Dir}...", _dataDirectory);
        await SeedItemsAsync(conn);
        await SeedSkillsAsync(conn);
        await SeedMonstersAsync(conn);
        _logger.LogInformation("Seed complete.");
    }

    private async Task SeedItemsAsync(SqliteConnection conn)
    {
        var path = Path.Combine(_dataDirectory, "items.json");
        if (!File.Exists(path)) { _logger.LogWarning("items.json not found at {Path}", path); return; }

        var items = JsonSerializer.Deserialize<List<Item>>(await File.ReadAllTextAsync(path), JsonOptions) ?? [];
        _logger.LogDebug("Upserting {Count} items...", items.Count);

        using var tx = conn.BeginTransaction();
        foreach (var item in items)
        {
            await conn.ExecuteAsync(@"
INSERT OR REPLACE INTO Items
    (Name, Grade, Category, SubCategory, AttackPower, DefensePower, MagicPower,
     RequiredLevel, Description, ImageUrl, IsTradable, Price)
VALUES
    (@Name, @Grade, @Category, @SubCategory, @AttackPower, @DefensePower, @MagicPower,
     @RequiredLevel, @Description, @ImageUrl, @IsTradable, @Price)", item, tx);
        }
        tx.Commit();
        _logger.LogInformation("{Count} items seeded.", items.Count);
    }

    private async Task SeedSkillsAsync(SqliteConnection conn)
    {
        var path = Path.Combine(_dataDirectory, "skills.json");
        if (!File.Exists(path)) { _logger.LogWarning("skills.json not found at {Path}", path); return; }

        var skills = JsonSerializer.Deserialize<List<Skill>>(await File.ReadAllTextAsync(path), JsonOptions) ?? [];
        _logger.LogDebug("Upserting {Count} skills...", skills.Count);

        using var tx = conn.BeginTransaction();
        foreach (var skill in skills)
        {
            await conn.ExecuteAsync(@"
INSERT OR REPLACE INTO Skills
    (Name, Class, Type, Element, MinDamage, MaxDamage, MpCost,
     CooldownSeconds, Range, MaxLevel, Description, IsUltimate)
VALUES
    (@Name, @Class, @Type, @Element, @MinDamage, @MaxDamage, @MpCost,
     @CooldownSeconds, @Range, @MaxLevel, @Description, @IsUltimate)", skill, tx);
        }
        tx.Commit();
        _logger.LogInformation("{Count} skills seeded.", skills.Count);
    }

    private async Task SeedMonstersAsync(SqliteConnection conn)
    {
        var path = Path.Combine(_dataDirectory, "monsters.json");
        if (!File.Exists(path)) { _logger.LogWarning("monsters.json not found at {Path}", path); return; }

        var monsters = JsonSerializer.Deserialize<List<Monster>>(await File.ReadAllTextAsync(path), JsonOptions) ?? [];
        _logger.LogDebug("Upserting {Count} monsters...", monsters.Count);

        using var tx = conn.BeginTransaction();
        foreach (var monster in monsters)
        {
            await conn.ExecuteAsync(@"
INSERT OR REPLACE INTO Monsters
    (Name, Grade, Element, Location, Level, Hp, AttackMin, AttackMax,
     Defense, MagicDefense, ExpReward, DropItems, Description, IsBoss)
VALUES
    (@Name, @Grade, @Element, @Location, @Level, @Hp, @AttackMin, @AttackMax,
     @Defense, @MagicDefense, @ExpReward, @DropItems, @Description, @IsBoss)", monster, tx);
        }
        tx.Commit();
        _logger.LogInformation("{Count} monsters seeded.", monsters.Count);
    }
}
