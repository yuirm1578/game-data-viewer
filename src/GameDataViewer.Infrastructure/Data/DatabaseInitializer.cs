using Dapper;
using Microsoft.Extensions.Logging;
using GameDataViewer.Infrastructure.Data;

namespace GameDataViewer.Infrastructure.Data;

/// <summary>DB 스키마 초기화 및 시드 데이터 적재</summary>
public class DatabaseInitializer
{
    private readonly DatabaseContext _db;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(DatabaseContext db, ILogger<DatabaseInitializer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public void Initialize()
    {
        _logger.LogInformation("Initializing database schema...");
        using var conn = _db.CreateConnection();
        CreateTables(conn);
        _logger.LogInformation("Database schema ready.");
    }

    private static void CreateTables(Microsoft.Data.Sqlite.SqliteConnection conn)
    {
        conn.Execute(@"
CREATE TABLE IF NOT EXISTS Items (
    Id            INTEGER PRIMARY KEY AUTOINCREMENT,
    Name          TEXT    NOT NULL,
    Grade         TEXT    NOT NULL DEFAULT '일반',
    Category      TEXT    NOT NULL DEFAULT '기타',
    SubCategory   TEXT    NOT NULL DEFAULT '',
    AttackPower   INTEGER NOT NULL DEFAULT 0,
    DefensePower  INTEGER NOT NULL DEFAULT 0,
    MagicPower    INTEGER NOT NULL DEFAULT 0,
    RequiredLevel INTEGER NOT NULL DEFAULT 1,
    Description   TEXT    NOT NULL DEFAULT '',
    ImageUrl      TEXT    NOT NULL DEFAULT '',
    IsTradable    INTEGER NOT NULL DEFAULT 1,
    Price         INTEGER NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS Skills (
    Id               INTEGER PRIMARY KEY AUTOINCREMENT,
    Name             TEXT    NOT NULL,
    Class            TEXT    NOT NULL DEFAULT '공용',
    Type             TEXT    NOT NULL DEFAULT '액티브',
    Element          TEXT    NOT NULL DEFAULT '무',
    MinDamage        INTEGER NOT NULL DEFAULT 0,
    MaxDamage        INTEGER NOT NULL DEFAULT 0,
    MpCost           INTEGER NOT NULL DEFAULT 0,
    CooldownSeconds  REAL    NOT NULL DEFAULT 0,
    Range            INTEGER NOT NULL DEFAULT 1,
    MaxLevel         INTEGER NOT NULL DEFAULT 10,
    Description      TEXT    NOT NULL DEFAULT '',
    IsUltimate       INTEGER NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS Monsters (
    Id           INTEGER PRIMARY KEY AUTOINCREMENT,
    Name         TEXT    NOT NULL,
    Grade        TEXT    NOT NULL DEFAULT '일반',
    Element      TEXT    NOT NULL DEFAULT '무',
    Location     TEXT    NOT NULL DEFAULT '미정',
    Level        INTEGER NOT NULL DEFAULT 1,
    Hp           INTEGER NOT NULL DEFAULT 100,
    AttackMin    INTEGER NOT NULL DEFAULT 1,
    AttackMax    INTEGER NOT NULL DEFAULT 1,
    Defense      INTEGER NOT NULL DEFAULT 0,
    MagicDefense INTEGER NOT NULL DEFAULT 0,
    ExpReward    INTEGER NOT NULL DEFAULT 0,
    DropItems    TEXT    NOT NULL DEFAULT '[]',
    Description  TEXT    NOT NULL DEFAULT '',
    IsBoss       INTEGER NOT NULL DEFAULT 0
);

CREATE INDEX IF NOT EXISTS IX_Items_Grade    ON Items(Grade);
CREATE INDEX IF NOT EXISTS IX_Items_Category ON Items(Category);
CREATE INDEX IF NOT EXISTS IX_Items_Name     ON Items(Name);

CREATE INDEX IF NOT EXISTS IX_Skills_Class   ON Skills(Class);
CREATE INDEX IF NOT EXISTS IX_Skills_Type    ON Skills(Type);
CREATE INDEX IF NOT EXISTS IX_Skills_Name    ON Skills(Name);

CREATE INDEX IF NOT EXISTS IX_Monsters_Grade    ON Monsters(Grade);
CREATE INDEX IF NOT EXISTS IX_Monsters_Location ON Monsters(Location);
CREATE INDEX IF NOT EXISTS IX_Monsters_Name     ON Monsters(Name);
");
    }
}
