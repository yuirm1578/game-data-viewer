using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace GameDataViewer.Infrastructure.Data;

/// <summary>SQLite DB 파일 경로 관리 및 연결 팩토리</summary>
public class DatabaseContext
{
    private readonly string _connectionString;
    private readonly ILogger<DatabaseContext> _logger;
    public string DbPath { get; }

    public DatabaseContext(ILogger<DatabaseContext> logger, string? dbPath = null)
    {
        _logger = logger;
        DbPath = dbPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GameDataViewer", "gamedata.db");

        // :memory: 또는 인메모리 URI는 디렉터리 생성 불필요
        if (!DbPath.StartsWith(':'))
        {
            var dir = Path.GetDirectoryName(DbPath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
        }

        _connectionString = $"Data Source={DbPath}";
        _logger.LogInformation("DatabaseContext initialized. DbPath={DbPath}", DbPath);
    }

    public SqliteConnection CreateConnection()
    {
        var conn = new SqliteConnection(_connectionString);
        conn.Open();
        // WAL 모드 + 외래키
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "PRAGMA journal_mode=WAL; PRAGMA foreign_keys=ON; PRAGMA synchronous=NORMAL;";
        cmd.ExecuteNonQuery();
        return conn;
    }

    /// <summary>앱 종료 시 WAL 파일을 체크포인트하여 db 파일에 병합</summary>
    public void Checkpoint()
    {
        try
        {
            using var conn = CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "PRAGMA wal_checkpoint(TRUNCATE);";
            cmd.ExecuteNonQuery();
            _logger.LogDebug("WAL checkpoint completed.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "WAL checkpoint failed.");
        }
    }
}
