using Microsoft.Data.Sqlite;

namespace NRMS.Infrastructure.Persistence.Sqlite;

public sealed class SqliteDb
{
    private readonly string _connectionString;

    public SqliteDb(string dbFilePath)
    {
        if (string.IsNullOrWhiteSpace(dbFilePath))
            throw new ArgumentException("Database file path is required.", nameof(dbFilePath));

        var dir = Path.GetDirectoryName(dbFilePath);
        if (!string.IsNullOrWhiteSpace(dir))
            Directory.CreateDirectory(dir);

        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = dbFilePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared
        }.ToString();
    }

    public SqliteConnection OpenConnection()
    {
        var conn = new SqliteConnection(_connectionString);
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "PRAGMA foreign_keys = ON;";
        cmd.ExecuteNonQuery();

        return conn;
    }

    public void EnsureCreated()
    {
        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = SqliteSchema.CreateSchemaSql;
        cmd.ExecuteNonQuery();

        using var tx = conn.BeginTransaction();
        using var update = conn.CreateCommand();
        update.Transaction = tx;
        update.CommandText = "UPDATE __schema_info SET SchemaVersion = @v;";
        update.Parameters.AddWithValue("@v", SqliteSchema.SchemaVersion);
        update.ExecuteNonQuery();
        tx.Commit();
    }
}
